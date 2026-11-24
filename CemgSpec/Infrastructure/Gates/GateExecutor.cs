using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CemgSpec.Application;
using CemgSpec.Domain;
using CemgSpec.Infrastructure.Documents;
using CemgSpec.Infrastructure.Execution;

namespace CemgSpec.Infrastructure.Gates;

public sealed record GateExecutionMetadata(
    string ToolIdentifier,
    string CoreVersion,
    string CoreSha256,
    string PolicySha256,
    string PythonVersion,
    string PythonSha256,
    int BrokerReadCount);

public sealed class GateExecutor
{
    private static readonly HashSet<string> ForbiddenExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".razor", ".cshtml", ".csproj", ".slnx", ".sln", ".fs", ".vb",
        ".js", ".ts", ".tsx", ".jsx", ".py", ".ps1", ".psm1", ".bat", ".cmd", ".sh",
        ".exe", ".dll", ".msi", ".config", ".xml", ".yml", ".yaml", ".env", ".pfx", ".key", ".pem"
    };

    private static readonly HashSet<string> ForbiddenConfigFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "package.json", "package-lock.json", "appsettings.json", "launchSettings.json",
        "global.json", "Directory.Build.props", "Directory.Packages.props"
    };

    private readonly GatePackage package;
    private readonly ExecutionLimits limits;

    public GateExecutor(GatePackage? package = null, ExecutionLimits? limits = null)
    {
        this.package = package ?? new GatePackage();
        this.limits = limits ?? new ExecutionLimits();
    }

    public async Task<GateExecutionMetadata?> ExecuteAsync(
        OpenProjectContext context,
        GateCandidate candidate,
        VerificationRun run,
        CancellationToken cancellationToken = default,
        Action<Guid>? beforeBrokerRead = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(run);

        if (run.Type != VerificationType.CheckGate)
            throw new InvalidOperationException("GateExecutor only executes CheckGate runs.");
        if (run.State == RunState.Blocked)
            return null;
        if (run.State != RunState.Preparing)
            throw new InvalidOperationException($"GateExecutor expects run in Preparing state, actual {run.State}.");

        var originPath = context.Catalog.FinalOriginPath;
        var pkgStatus = package.VerifyPackage(originPath);
        if (!pkgStatus.IsValid || pkgStatus.CorePath is null || pkgStatus.PolicyPath is null)
        {
            run.MarkFailed(pkgStatus.Message ?? "Pacote confiável check_gate inválido.");
            return null;
        }

        var runnerPath = Path.Combine(package.PackageRoot, "runner.py");
        var catalogPathScript = Path.Combine(package.PackageRoot, "catalog_path.py");
        if (!File.Exists(runnerPath) || !File.Exists(catalogPathScript))
        {
            run.MarkFailed("Scripts confiáveis do runner CemgSpec (runner.py / catalog_path.py) não encontrados.");
            return null;
        }

        var runtime = package.ResolvePythonRuntime(originPath);
        if (!runtime.IsAvailable || runtime.ExecutablePath is null || runtime.Sha256 is null)
        {
            run.MarkFailed(runtime.Message ?? "Runtime Python confiável indisponível.");
            return null;
        }

        var feature = context.Session.RequireFeature(candidate.FeatureId);
        var broker = new DocumentBroker(context.Catalog);
        var generation = candidate.CatalogGeneration;

        // Build feature-relative virtual catalog map (authorized documentary entries only; NEVER PresentationAsset or code).
        var featureEntries = context.Catalog.Entries
            .Where(e => e.FeatureId == candidate.FeatureId)
            .ToArray();

        var featurePrefix = feature.SpecKitRelativeDirectory.Replace('\\', '/').TrimEnd('/') + "/";
        var catalogMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var presentationAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string? recordFeatureRelative = null;

        foreach (var entry in featureEntries)
        {
            var normalizedFull = entry.RelativePath.Replace('\\', '/');
            if (!normalizedFull.StartsWith(featurePrefix, StringComparison.OrdinalIgnoreCase))
                continue;
            var rel = normalizedFull[featurePrefix.Length..];
            if (entry.Kind == CatalogKind.PresentationAsset)
            {
                presentationAssets.Add(rel);
                continue;
            }
            catalogMap[rel] = entry.DocumentId;
            if (entry.DocumentId == candidate.RecordDocumentId)
                recordFeatureRelative = rel;
        }

        if (recordFeatureRelative is null)
        {
            run.BlockPreparation(
                "Registro de gate não pertence ao catálogo documental autorizado da funcionalidade.",
                ["Nenhum arquivo fora do catálogo documental é lido pelo CemgSpec."]);
            return null;
        }

        // Preflight full evidence graph via DocumentBroker BEFORE launching Python.
        var preflightError = PreflightEvidenceGraph(
            context,
            broker,
            featurePrefix,
            candidate.RecordDocumentId,
            generation,
            catalogMap,
            presentationAssets);

        if (preflightError is not null)
        {
            if (preflightError.IsStale)
                run.MarkStale();
            run.BlockPreparation(
                preflightError.Message,
                [
                    "O preflight de escopo bloqueia referências a código, configuração, caminhos externos, traversal, junções ou assets não documentais sem ler destinos proibidos.",
                    "Nenhum resultado parcial de prontidão é emitido quando uma evidência exigida é inválida ou fora do escopo."
                ]);
            return null;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            run.RequestCancel();
            run.FinishCancellation(terminationConfirmed: true, "Execução cancelada antes do início do processo Python.");
            return null;
        }

        // Reconfirm Python binary hash just before launching.
        if (!package.VerifyRuntimeUnchanged(runtime, originPath))
        {
            run.MarkFailed("Hash do runtime Python foi alterado antes da execução.");
            return null;
        }

        var externalWorkDir = Path.Combine(Path.GetTempPath(), "CemgSpec-GateWork");
        Directory.CreateDirectory(externalWorkDir);
        using (var originHandle = WindowsReadHandle.Open(originPath, true))
        using (var workHandle = WindowsReadHandle.Open(externalWorkDir, true))
        {
            var finalOrigin = WindowsReadHandle.FinalPath(originHandle);
            var finalWork = WindowsReadHandle.FinalPath(workHandle);
            if (string.Equals(finalOrigin, finalWork, StringComparison.OrdinalIgnoreCase) ||
                WindowsReadHandle.Contains(finalOrigin, finalWork))
            {
                run.MarkFailed("Diretório de trabalho do runner não pode coincidir com a origem auditada.");
                return null;
            }
        }

        var toolLabel = $"{pkgStatus.ToolIdentifier} (Python {runtime.Version})";
        run.BeginRunning(toolLabel);

        var workToken = context.WorkToken;
        using var channel = new ExecutionChannel(run.RunId, generation);
        using var deadlineCts = new CancellationTokenSource(limits.MaxDuration);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, deadlineCts.Token, workToken);

        var startInfo = new ProcessStartInfo(runtime.ExecutablePath)
        {
            WorkingDirectory = externalWorkDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        startInfo.ArgumentList.Add("-I");
        startInfo.ArgumentList.Add("-B");
        startInfo.ArgumentList.Add(runnerPath);

        startInfo.Environment.Clear();
        foreach (var name in new[] { "SystemRoot", "WINDIR", "TEMP", "TMP" })
        {
            if (Environment.GetEnvironmentVariable(name) is { } value)
                startInfo.Environment[name] = value;
        }
        startInfo.Environment["PATH"] = Environment.SystemDirectory;
        startInfo.Environment["PYTHONUTF8"] = "1";
        startInfo.Environment["PYTHONDONTWRITEBYTECODE"] = "1";

        using var process = new Process { StartInfo = startInfo };
        if (!process.Start())
        {
            run.MarkFailed("Não foi possível iniciar o processo confiável do runner Python.");
            return null;
        }

        var stderrTask = ReadBoundedStderrAsync(process.StandardError, linkedCts.Token);
        var brokerReadCount = 0;
        var stdoutTranscript = new StringBuilder();
        JsonDocument? finalResultDoc = null;
        string? protocolError = null;
        var staleDetected = false;

        try
        {
            // Send start payload (contains ONLY virtual catalog paths -> opaque DocumentIds, NEVER originPath).
            var startPayload = new
            {
                op = "start",
                run_id = channel.RunId.ToString("D"),
                token = channel.Token,
                catalog_generation = generation,
                stage = candidate.Stage.ToCliName(),
                feature_virtual_root = feature.SpecKitRelativeDirectory.Replace('\\', '/'),
                record_relative_path = recordFeatureRelative,
                expected_revision = candidate.ExpectedRevision,
                core_path = pkgStatus.CorePath,
                policy_path = pkgStatus.PolicyPath,
                expected_core_sha256 = pkgStatus.CoreSha256,
                expected_policy_sha256 = pkgStatus.PolicySha256,
                catalog_map = catalogMap.ToDictionary(kv => kv.Key, kv => kv.Value.ToString("D"))
            };

            await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(startPayload).AsMemory(), linkedCts.Token);
            await process.StandardInput.FlushAsync(linkedCts.Token);

            while (true)
            {
                var line = await process.StandardOutput.ReadLineAsync(linkedCts.Token);
                if (line is null) break;

                limits.CheckOutput(stdoutTranscript.Length + line.Length);
                if (stdoutTranscript.Length > 0) stdoutTranscript.AppendLine();
                stdoutTranscript.Append(line);

                JsonDocument message;
                try
                {
                    message = JsonDocument.Parse(line);
                }
                catch (JsonException ex)
                {
                    protocolError = $"Saída JSON inválida do runner: {ex.Message}";
                    break;
                }

                var op = message.RootElement.TryGetProperty("op", out var opProp) ? opProp.GetString() : null;
                if (op == "read")
                {
                    using (message)
                    {
                        var reqRunId = message.RootElement.TryGetProperty("run_id", out var rProp) && Guid.TryParse(rProp.GetString(), out var parsedRun) ? parsedRun : Guid.Empty;
                        var reqToken = message.RootElement.TryGetProperty("token", out var tProp) ? tProp.GetString() ?? "" : "";
                        var reqGen = message.RootElement.TryGetProperty("catalog_generation", out var gProp) && gProp.TryGetInt64(out var parsedGen) ? parsedGen : -1;
                        var docId = message.RootElement.TryGetProperty("document_id", out var dProp) && Guid.TryParse(dProp.GetString(), out var parsedDoc) ? parsedDoc : Guid.Empty;

                        if (!channel.Authorize(reqRunId, reqToken, reqGen))
                        {
                            await WriteBrokerErrorAsync(process.StandardInput, "Unauthorized", "Credencial do canal inválida.", linkedCts.Token);
                            protocolError = "Requisição não autorizada recebida do runner.";
                            break;
                        }

                        brokerReadCount++;
                        limits.CheckGraph(brokerReadCount, 0);

                        try
                        {
                            beforeBrokerRead?.Invoke(docId);
                            if (context.Catalog.HasPendingChanges || context.Catalog.Generation != generation)
                                throw new DocumentAccessException("Changed");

                            var snapshot = broker.Read(context.Session.ProjectId, docId, generation);
                            var reply = JsonSerializer.Serialize(new
                            {
                                ok = true,
                                content_base64 = Convert.ToBase64String(snapshot.CopyBytes()),
                                sha256 = snapshot.ContentHash,
                                snapshot_id = snapshot.SnapshotId.ToString("D")
                            });
                            await process.StandardInput.WriteLineAsync(reply.AsMemory(), linkedCts.Token);
                            await process.StandardInput.FlushAsync(linkedCts.Token);
                        }
                        catch (DocumentAccessException ex)
                        {
                            if (ex.Code is "Changed" or "Missing")
                                staleDetected = true;
                            await WriteBrokerErrorAsync(process.StandardInput, ex.Code, $"Broker negou leitura ({ex.Code}).", linkedCts.Token);
                        }
                        catch (LimitExceededException ex)
                        {
                            await WriteBrokerErrorAsync(process.StandardInput, "LimitExceeded", $"Limite técnico atingido ({ex.Dimension}).", linkedCts.Token);
                        }
                    }
                }
                else if (op is "result" or "error")
                {
                    finalResultDoc = message;
                    break;
                }
                else
                {
                    message.Dispose();
                    protocolError = $"Operação desconhecida recebida do runner: {op ?? "(null)"}";
                    break;
                }
            }

            try { process.StandardInput.Close(); } catch { }
            await process.WaitForExitAsync(linkedCts.Token);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested || workToken.IsCancellationRequested)
        {
            var terminated = await TerminateProcessTreeAsync(process);
            var stderr = await SafeAwaitAsync(stderrTask);
            var reason = cancellationToken.IsCancellationRequested
                ? "Execução de check_gate cancelada pelo usuário."
                : "Execução de check_gate cancelada por alteração de documento ou troca de projeto.";
            if (workToken.IsCancellationRequested)
                run.MarkStale();
            if (!run.CancelRequested)
                run.RequestCancel();
            run.FinishCancellation(terminated, reason, process.HasExited ? process.ExitCode : null, stdoutTranscript.ToString(), stderr);
            finalResultDoc?.Dispose();
            return null;
        }
        catch (OperationCanceledException) when (deadlineCts.IsCancellationRequested)
        {
            var terminated = await TerminateProcessTreeAsync(process);
            var stderr = await SafeAwaitAsync(stderrTask);
            run.MarkFailed(
                $"Tempo limite técnico ({limits.MaxDuration.TotalSeconds:F0}s) excedido ao executar check_gate.",
                process.HasExited ? process.ExitCode : null,
                stdoutTranscript.ToString(),
                stderr,
                terminationConfirmed: terminated);
            finalResultDoc?.Dispose();
            return null;
        }
        catch (LimitExceededException ex)
        {
            var terminated = await TerminateProcessTreeAsync(process);
            var stderr = await SafeAwaitAsync(stderrTask);
            run.MarkFailed(
                $"Limite técnico atingido ({ex.Dimension}); resultado não foi truncado nem promovido a READY.",
                process.HasExited ? process.ExitCode : null,
                stdoutTranscript.ToString(),
                stderr,
                terminationConfirmed: terminated);
            finalResultDoc?.Dispose();
            return null;
        }

        var finalStderr = await SafeAwaitAsync(stderrTask);
        var exitCode = process.HasExited ? process.ExitCode : -1;

        if (staleDetected || context.Catalog.HasPendingChanges || context.Catalog.Generation != generation)
            run.MarkStale();

        if (protocolError is not null || finalResultDoc is null)
        {
            finalResultDoc?.Dispose();
            run.MarkFailed(
                protocolError ?? "Runner encerrou sem retornar resultado estruturado válido.",
                exitCode,
                stdoutTranscript.ToString(),
                finalStderr);
            return null;
        }

        using (finalResultDoc)
        {
            var root = finalResultDoc.RootElement;
            var op = root.TryGetProperty("op", out var opProp) ? opProp.GetString() : null;
            if (op == "error")
            {
                var errCode = root.TryGetProperty("code", out var cProp) ? cProp.GetString() : "RunnerError";
                var errMsg = root.TryGetProperty("message", out var mProp) ? mProp.GetString() : "Falha no runner.";
                run.MarkFailed($"{errCode}: {errMsg}", exitCode, stdoutTranscript.ToString(), finalStderr);
                return null;
            }

            var scopeViolation = root.TryGetProperty("scope_violation", out var svProp) && svProp.GetBoolean();
            var brokerErrorCode = root.TryGetProperty("broker_error_code", out var beProp) && beProp.ValueKind == JsonValueKind.String
                ? beProp.GetString()
                : null;
            var errors = ReadStringArray(root, "errors");

            if (scopeViolation || !string.IsNullOrWhiteSpace(brokerErrorCode))
            {
                if (brokerErrorCode is "Changed" or "Missing")
                    run.MarkStale();
                run.MarkFailed(
                    $"Execução interrompida pelo controle de leitura/escopo ({brokerErrorCode ?? "OutOfScope"}): {string.Join("; ", errors)}",
                    exitCode,
                    JsonSerializer.Serialize(new { result = "BLOCKED", errors }),
                    finalStderr,
                    terminationConfirmed: true,
                    limitations:
                    [
                        "O broker revalidou handle, contenção, junções e hash no momento real da leitura.",
                        "Nenhum destino fora do catálogo autorizado foi lido e nenhum READY parcial foi emitido."
                    ]);
                return null;
            }

            var outcomeText = root.TryGetProperty("outcome", out var outProp) ? outProp.GetString() : null;
            GateSemanticResult semantic;
            if (outcomeText == "READY" && exitCode == 0)
                semantic = GateSemanticResult.Ready;
            else if (outcomeText == "DEV_IMPOSSIBILITY_RECORDED" && exitCode == 0)
                semantic = GateSemanticResult.DevImpossibilityRecorded;
            else if (outcomeText == "BLOCKED" && exitCode == 1)
                semantic = GateSemanticResult.Blocked;
            else
            {
                run.MarkFailed(
                    $"Resultado/exit inconsistente do core (outcome={outcomeText ?? "null"}, exit={exitCode}).",
                    exitCode,
                    stdoutTranscript.ToString(),
                    finalStderr);
                return null;
            }

            var findings = errors.Select((error, idx) => new Finding(
                Guid.NewGuid(),
                run.RunId,
                Category: "GateValidation",
                Severity: FindingSeverity.High,
                Explanation: error,
                SourceLocations: [new DocumentSourceLocation(candidate.RecordDocumentId, featurePrefix + recordFeatureRelative, 1)],
                Suggestion: "Corrigir o registro ou a evidência documental na origem e executar o gate novamente.",
                ContextFingerprint: $"{candidate.Stage.ToCliName()}:{candidate.ExpectedRevision}:{idx}:{error}")).ToArray();

            var limitations = new List<string>
            {
                $"Executado com {pkgStatus.ToolIdentifier} (core SHA-256: {pkgStatus.CoreSha256![..12]}..., política SHA-256: {pkgStatus.PolicySha256![..12]}...).",
                $"Revisão esperada ({candidate.RevisionSourceLabel}): {candidate.ExpectedRevision}.",
                "check_gate valida integridade e estado registrados nos documentos; não autentica decisão humana nem testa implementação.",
                "Resultado mantido apenas em memória até ação explícita de Salvar em destino externo."
            };
            if (run.FocusRequirementKey is not null)
            {
                limitations.Add($"Foco da ficha em {run.FocusRequirementKey.LocalId}: este resultado corresponde ao gate da funcionalidade/etapa ({candidate.Stage.ToCliName()}), nunca a um gate individual do requisito.");
            }

            var cleanNativeJson = JsonSerializer.Serialize(new { result = outcomeText, errors });
            run.CompleteGate(semantic, exitCode, cleanNativeJson, finalStderr, findings, limitations);

            return new GateExecutionMetadata(
                pkgStatus.ToolIdentifier,
                pkgStatus.CoreVersion,
                pkgStatus.CoreSha256!,
                pkgStatus.PolicySha256!,
                runtime.Version!,
                runtime.Sha256!,
                brokerReadCount);
        }
    }

    private sealed record PreflightBlock(string Message, bool IsStale = false);

    private PreflightBlock? PreflightEvidenceGraph(
        OpenProjectContext context,
        DocumentBroker broker,
        string featurePrefix,
        Guid recordDocumentId,
        long generation,
        Dictionary<string, Guid> catalogMap,
        HashSet<string> presentationAssets)
    {
        DocumentSnapshot recordSnapshot;
        try
        {
            recordSnapshot = broker.Read(context.Session.ProjectId, recordDocumentId, generation);
        }
        catch (DocumentAccessException ex)
        {
            return new PreflightBlock($"Não foi possível ler o registro de gate ({ex.Code}).", IsStale: ex.Code is "Changed" or "Missing");
        }

        JsonDocument recordDoc;
        try
        {
            recordDoc = JsonDocument.Parse(recordSnapshot.CopyBytes());
        }
        catch (JsonException)
        {
            // Malformed JSON is handled by core check_gate as BLOCKED, not a scope violation.
            return null;
        }

        using (recordDoc)
        {
            var root = recordDoc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var nodes = 1;

            if (root.TryGetProperty("evidence", out var evidenceProp) && evidenceProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in evidenceProp.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object) continue;
                    var path = item.TryGetProperty("path", out var pProp) && pProp.ValueKind == JsonValueKind.String ? pProp.GetString() : null;
                    var kind = item.TryGetProperty("kind", out var kProp) && kProp.ValueKind == JsonValueKind.String ? kProp.GetString() : null;
                    if (string.IsNullOrWhiteSpace(path)) continue;

                    var check = ValidateGraphPath(context, featurePrefix, path, catalogMap, presentationAssets, visited, ref nodes, depth: 1);
                    if (check is not null) return check;

                    var normalized = NormalizeFeaturePath(path);
                    if (normalized is null || !catalogMap.TryGetValue(normalized, out var childDocId))
                        continue;

                    if (kind is "test-run" or "product-execution")
                    {
                        DocumentSnapshot childSnapshot;
                        try
                        {
                            childSnapshot = broker.Read(context.Session.ProjectId, childDocId, generation);
                        }
                        catch (DocumentAccessException ex)
                        {
                            return new PreflightBlock($"Evidência '{path}' indisponível no broker ({ex.Code}).", IsStale: ex.Code is "Changed" or "Missing");
                        }

                        try
                        {
                            using var childJson = JsonDocument.Parse(childSnapshot.CopyBytes());
                            if (childJson.RootElement.ValueKind != JsonValueKind.Object) continue;

                            if (kind == "test-run")
                            {
                                foreach (var field in new[] { "stdout", "stderr" })
                                {
                                    if (childJson.RootElement.TryGetProperty(field, out var logObj) &&
                                        logObj.ValueKind == JsonValueKind.Object &&
                                        logObj.TryGetProperty("path", out var lp) &&
                                        lp.ValueKind == JsonValueKind.String &&
                                        !string.IsNullOrWhiteSpace(lp.GetString()))
                                    {
                                        var nestedCheck = ValidateGraphPath(context, featurePrefix, lp.GetString()!, catalogMap, presentationAssets, visited, ref nodes, depth: 2);
                                        if (nestedCheck is not null) return nestedCheck;
                                    }
                                }
                            }
                            else if (kind == "product-execution" &&
                                     childJson.RootElement.TryGetProperty("artifacts", out var artifactsProp) &&
                                     artifactsProp.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var art in artifactsProp.EnumerateArray())
                                {
                                    if (art.ValueKind == JsonValueKind.Object &&
                                        art.TryGetProperty("path", out var ap) &&
                                        ap.ValueKind == JsonValueKind.String &&
                                        !string.IsNullOrWhiteSpace(ap.GetString()))
                                    {
                                        var nestedCheck = ValidateGraphPath(context, featurePrefix, ap.GetString()!, catalogMap, presentationAssets, visited, ref nodes, depth: 2);
                                        if (nestedCheck is not null) return nestedCheck;
                                    }
                                }
                            }
                        }
                        catch (JsonException)
                        {
                            // Malformed child JSON is validated by core check_gate.
                        }
                    }
                }
            }

            if (root.TryGetProperty("human_decisions", out var decisionsProp) && decisionsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var dec in decisionsProp.EnumerateArray())
                {
                    if (dec.ValueKind == JsonValueKind.Object &&
                        dec.TryGetProperty("source", out var sp) &&
                        sp.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(sp.GetString()))
                    {
                        var check = ValidateGraphPath(context, featurePrefix, sp.GetString()!, catalogMap, presentationAssets, visited, ref nodes, depth: 1);
                        if (check is not null) return check;
                    }
                }
            }

            if (root.TryGetProperty("human_confirmation", out var confProp) &&
                confProp.ValueKind == JsonValueKind.Object &&
                confProp.TryGetProperty("source", out var csp) &&
                csp.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(csp.GetString()))
            {
                var check = ValidateGraphPath(context, featurePrefix, csp.GetString()!, catalogMap, presentationAssets, visited, ref nodes, depth: 1);
                if (check is not null) return check;
            }
        }

        return null;
    }

    private PreflightBlock? ValidateGraphPath(
        OpenProjectContext context,
        string featurePrefix,
        string rawPath,
        Dictionary<string, Guid> catalogMap,
        HashSet<string> presentationAssets,
        HashSet<string> visited,
        ref int nodes,
        int depth)
    {
        nodes++;
        try { limits.CheckGraph(nodes, depth); }
        catch (LimitExceededException ex)
        {
            return new PreflightBlock($"Limite técnico do grafo de evidências excedido ({ex.Dimension}).");
        }

        if (rawPath.Contains('\0'))
            return new PreflightBlock($"Caminho inválido contendo byte nulo: '{rawPath}'.");

        if (Uri.TryCreate(rawPath, UriKind.Absolute, out var uri) && !uri.IsFile)
            return new PreflightBlock($"Referência externa/URI proibida no gate offline: '{rawPath}'.");

        var replaced = rawPath.Replace('\\', '/');
        if (Path.IsPathRooted(rawPath) || replaced.StartsWith('/') || (replaced.Length >= 2 && replaced[1] == ':'))
            return new PreflightBlock($"Caminho absoluto externo proibido como evidência de gate: '{rawPath}'.");

        var normalized = NormalizeFeaturePath(replaced);
        if (normalized is null)
            return new PreflightBlock($"Referência fora da funcionalidade (traversal) bloqueada no preflight: '{rawPath}'.");

        var fileName = Path.GetFileName(normalized);
        var ext = Path.GetExtension(normalized);
        if (ForbiddenExtensions.Contains(ext) || ForbiddenConfigFiles.Contains(fileName))
            return new PreflightBlock($"Referência a código ou configuração ('{rawPath}') bloqueada no preflight; o CemgSpec V1 não lê código-fonte.");

        if (presentationAssets.Contains(normalized))
            return new PreflightBlock($"Asset não documental ('{rawPath}') não pode substituir evidência documental exigida no gate.");

        // Check without opening content whether the target or any ancestor under feature is a reparse point / junction.
        if (!catalogMap.ContainsKey(normalized))
        {
            var featureDiskDir = Path.Combine(context.Catalog.FinalOriginPath, featurePrefix.TrimEnd('/').Replace('/', Path.DirectorySeparatorChar));
            var candidateDiskPath = Path.Combine(featureDiskDir, normalized.Replace('/', Path.DirectorySeparatorChar));
            if (IsReparsePointOrJunctionOnPath(featureDiskDir, candidateDiskPath))
                return new PreflightBlock($"Junção ou link simbólico detectado em '{rawPath}'; leitura negada no preflight.");
            if (File.Exists(candidateDiskPath))
                return new PreflightBlock($"Arquivo '{rawPath}' não pertence ao catálogo documental autorizado da funcionalidade; leitura negada.");
        }

        visited.Add(normalized);
        return null;
    }

    private static string? NormalizeFeaturePath(string raw)
    {
        var segments = raw.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var stack = new List<string>();
        foreach (var seg in segments)
        {
            if (seg == ".") continue;
            if (seg == "..")
            {
                if (stack.Count == 0) return null;
                stack.RemoveAt(stack.Count - 1);
            }
            else
            {
                stack.Add(seg);
            }
        }
        return stack.Count == 0 ? null : string.Join('/', stack);
    }

    private static bool IsReparsePointOrJunctionOnPath(string rootDir, string targetPath)
    {
        try
        {
            var current = targetPath;
            while (!string.IsNullOrWhiteSpace(current) &&
                   current.Length >= rootDir.Length &&
                   !string.Equals(current, rootDir, StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(current) || Directory.Exists(current))
                {
                    if (File.GetAttributes(current).HasFlag(FileAttributes.ReparsePoint))
                        return true;
                }
                current = Path.GetDirectoryName(current);
            }
        }
        catch (IOException) { return true; }
        catch (UnauthorizedAccessException) { return true; }
        return false;
    }

    private static async Task WriteBrokerErrorAsync(StreamWriter stdin, string code, string message, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new { ok = false, code, message });
        await stdin.WriteLineAsync(payload.AsMemory(), ct);
        await stdin.FlushAsync(ct);
    }

    private async Task<string> ReadBoundedStderrAsync(StreamReader reader, CancellationToken ct)
    {
        var sb = new StringBuilder();
        var buffer = new char[2048];
        while (true)
        {
            int read;
            try { read = await reader.ReadAsync(buffer.AsMemory(), ct); }
            catch (OperationCanceledException) { return sb.ToString(); }
            if (read == 0) return sb.ToString();
            var remaining = Math.Max(0, limits.MaxOutputCharacters - sb.Length);
            if (remaining > 0) sb.Append(buffer, 0, Math.Min(read, remaining));
        }
    }

    private static async Task<bool> TerminateProcessTreeAsync(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            using var waitCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await process.WaitForExitAsync(waitCts.Token);
            return process.HasExited;
        }
        catch
        {
            return process.HasExited;
        }
    }

    private static async Task<string> SafeAwaitAsync(Task<string> task)
    {
        try { return await task; }
        catch { return string.Empty; }
    }

    private static string[] ReadStringArray(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return [];
        return arr.EnumerateArray()
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString() ?? string.Empty)
            .ToArray();
    }
}
