using System.ComponentModel;
using System.Text;
using System.Text.Json;
using SpecAudit.Application;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Documents;
using SpecAudit.Infrastructure.Execution;

namespace SpecAudit.Infrastructure.Agents;

public sealed class AntigravityAnalyzeAdapter
{
    public const string PolicyVersion = "v4";
    public const string PlatformFingerprint = "win-x64";
    public const string VerifiedAntigravityVersion = "1.5.13";
    public const string VerifiedAntigravitySha256 = "91bd7720100dec46acd5a852e8291e5704eb753f5801c625950a15dc414fdf0a";

    private const string RestrictedSettings = """
        {"agentMode":"","toolPermission":"strict","allowNonWorkspaceAccess":false,
         "historySize":0,"permissions":{"allow":[],"ask":[],
         "deny":["read_file(*)","write_file(*)","read_url(*)","execute_url(*)","download_url(*)","command(*)","mcp(*)"]}}
        """;

    private readonly AgentDiscovery discovery;
    private readonly Func<TrustedCommandDefinition, string, string, string, TimeSpan, CancellationToken,
        Task<CommandExecutionResult>> runner;

    public AntigravityAnalyzeAdapter() : this(new AgentDiscovery(), new TrustedCommandRunner().RunAsync) { }

    internal AntigravityAnalyzeAdapter(AgentDiscovery discovery,
        Func<TrustedCommandDefinition, string, string, string, TimeSpan, CancellationToken,
            Task<CommandExecutionResult>> runner)
    {
        this.discovery = discovery;
        this.runner = runner;
    }

    // Only the adapter harness calls this in T068. Product availability requires a separate protection assessment.
    internal async Task<AnalyzeExecutionMetadata?> ExecuteAsync(OpenProjectContext context, Guid featureId,
        AgentInstallation installation, VerificationRun run, CancellationToken cancellationToken = default)
    {
        if (installation.Provider != AgentProvider.Antigravity || run.Type != VerificationType.AnalyzeDocuments ||
            run.ProjectId != context.Session.ProjectId || run.FeatureId != featureId || run.AgentId != installation.AgentId)
            throw new InvalidOperationException("A execução não corresponde ao agente, projeto e funcionalidade preparados.");
        if (run.State == RunState.Blocked) return null;
        if (run.State != RunState.Preparing) throw new InvalidOperationException("A execução deve estar em preparação.");

        var feature = context.Session.RequireFeature(featureId);
        var origin = context.Catalog.FinalOriginPath;
        var workToken = context.WorkToken;
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(workToken, cancellationToken);
        if (lifetime.IsCancellationRequested)
        {
            if (workToken.IsCancellationRequested) run.MarkStale();
            run.BlockPreparation("Análise Antigravity cancelada antes da inicialização.");
            return null;
        }

        try
        {
            if (!installation.IsFound || installation.ExecutablePath is not { } executable ||
                installation.ExecutableSha256 is not { } sha256 ||
                !Path.GetFileName(executable).Equals("agy.exe", StringComparison.OrdinalIgnoreCase) ||
                !discovery.VerifyUnchanged(installation, origin))
            {
                run.BlockPreparation("Antigravity CLI (agy.exe) ausente, incompatível ou alterado.");
                return null;
            }

            var entries = context.Catalog.Entries;
            var required = new[] { CatalogKind.Constitution, CatalogKind.Spec, CatalogKind.Plan, CatalogKind.Tasks };
            var broker = new DocumentBroker(context.Catalog);
            var snapshots = new Dictionary<Guid, (CatalogEntry Entry, DocumentSnapshot Snapshot, string Text)>();
            using var channel = new ExecutionChannel(run.RunId, run.CatalogGeneration);
            foreach (var kind in required)
            {
                var entry = entries.SingleOrDefault(candidate => candidate.Kind == kind &&
                    (kind == CatalogKind.Constitution || candidate.FeatureId == featureId));
                if (entry is null || !run.InputSnapshotIds.Contains(entry.DocumentId))
                {
                    run.BlockPreparation("Pré-requisitos documentais ausentes na preparação. Nenhum arquivo foi criado na origem.");
                    return null;
                }
                if (!channel.Authorize(run.RunId, channel.Token, run.CatalogGeneration))
                    throw new DocumentAccessException("UnauthorizedChannel");
                var snapshot = broker.Read(context.Session.ProjectId, entry.DocumentId, run.CatalogGeneration);
                snapshots.Add(entry.DocumentId, (entry, snapshot, new UTF8Encoding(false, true).GetString(snapshot.CopyBytes())));
            }

            var prompt = CodexAnalyzeAdapter.BuildDocumentaryPrompt(feature, snapshots.Values, run.FocusRequirementKey);
            var input = JsonSerializer.Serialize(new { @event = "user", message = new { content = prompt } }) + "\n";
            if (Encoding.UTF8.GetByteCount(input) > 4 * 1024 * 1024)
                throw new LimitExceededException("input-bytes");

            using var temp = WindowsReadHandle.Open(Path.GetFullPath(Path.GetTempPath()), true);
            var stateParent = WindowsReadHandle.FinalPath(temp);
            using var report = WindowsReadHandle.Open(context.OutputDisplayPath, true);
            var reportPath = WindowsReadHandle.FinalPath(report);
            if (Within(origin, stateParent) || Within(reportPath, stateParent))
            {
                run.BlockPreparation("O diretório temporário precisa estar fora da origem e do destino dos relatórios.");
                return null;
            }
            if (context.Catalog.HasPendingChanges || context.Catalog.Generation != run.CatalogGeneration)
                throw new DocumentAccessException("Changed");
            lifetime.Token.ThrowIfCancellationRequested();

            using var directory = WindowsReadHandle.Open(Directory.CreateTempSubdirectory("SpecAudit-Antigravity-").FullName, true);
            var stateDirectory = WindowsReadHandle.FinalPath(directory);
            if (!Within(stateParent, stateDirectory) || Within(origin, stateDirectory) || Within(reportPath, stateDirectory))
                throw new InvalidOperationException("Diretório externo de execução inválido.");
            var label = $"speckit-analyze · Antigravity {installation.Version ?? "desconhecido"} (somente leitura)";
            var command = new TrustedCommandDefinition(VerificationType.AnalyzeDocuments, executable, sha256,
                ["--input-format", "stream-json", "--output-format", "stream-json", "--disable-slash-commands",
                    "--settings", RestrictedSettings, "--sandbox", "--json-schema", CodexAnalyzeAdapter.OutputSchemaJson]);

            run.BeginRunning(label);
            var result = await runner(command, input, stateDirectory, origin, TimeSpan.FromSeconds(120), lifetime.Token);
            if (workToken.IsCancellationRequested || context.Catalog.HasPendingChanges || context.Catalog.Generation != run.CatalogGeneration)
                run.MarkStale();
            if (result.Cancelled || lifetime.IsCancellationRequested)
            {
                run.RequestCancel();
                run.FinishCancellation(result.RootTerminationConfirmed, "Análise Antigravity cancelada.", result.ExitCode);
                return null;
            }
            if (!result.RootTerminationConfirmed || result.TimedOut || result.LimitExceeded || result.ExitCode != 0 ||
                AuthenticationRequired(result.Error))
            {
                var reason = AuthenticationRequired(result.Error) || AuthenticationRequired(result.Output)
                    ? "Autenticação Antigravity ausente ou inválida no perfil isolado."
                    : "Execução Antigravity falhou, excedeu os limites ou não concluiu.";
                run.MarkFailed(reason, result.ExitCode, terminationConfirmed: result.RootTerminationConfirmed);
                return null;
            }

            foreach (var id in snapshots.Keys)
                broker.Read(context.Session.ProjectId, id, run.CatalogGeneration);

            var payload = ExtractDocumentaryResult(result.Output);
            var disclosure = $"Adaptação documental somente leitura de speckit-analyze (Antigravity). " +
                $"Os quatro snapshots autorizados podem ser enviados ao provedor. Estado interno e cache externos: {stateDirectory}. " +
                "Conversas do agente não integram o histórico oficial.";
            var mapped = CodexAnalyzeAdapter.ValidateAndMapOutput(run, feature, payload, snapshots, installation, disclosure);
            var valid = mapped.SchemaValid && mapped.RejectedReferenceCount == 0;
            run.CompleteAnalyze(valid ? mapped.SemanticResult : AnalyzeSemanticResult.Inconclusive, 0,
                "", "", valid ? mapped.Findings : [], mapped.Limitations);
            return new(label, AgentProvider.Antigravity, installation.Version ?? "desconhecido", sha256,
                [stateDirectory], snapshots.Count, mapped.RejectedReferenceCount);
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
            if (workToken.IsCancellationRequested) run.MarkStale();
            if (run.State == RunState.Preparing)
                run.BlockPreparation("Análise Antigravity cancelada antes da inicialização.");
            else
            {
                run.RequestCancel();
                run.FinishCancellation(terminationConfirmed: true, "Análise Antigravity cancelada antes da execução do processo.");
            }
            return null;
        }
        catch (DocumentAccessException exception)
        {
            if (exception.Code == "Changed") run.MarkStale();
            run.MarkFailed($"Leitura documental interrompida pelo broker ({exception.Code}).");
            return null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or
            Win32Exception or InvalidOperationException or DecoderFallbackException or LimitExceededException)
        {
            run.MarkFailed("Falha local na preparação ou execução documental do Antigravity.",
                terminationConfirmed: run.State == RunState.Preparing);
            return null;
        }
    }

    private static bool Within(string root, string path) =>
        string.Equals(root, path, StringComparison.OrdinalIgnoreCase) || WindowsReadHandle.Contains(root, path);

    private static bool AuthenticationRequired(string text) =>
        text.Contains("authentication required", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("not authenticated", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("login required", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("401 Unauthorized", StringComparison.OrdinalIgnoreCase);

    private static string ExtractDocumentaryResult(string output)
    {
        string? payload = null;
        try
        {
            foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                using var eventDocument = JsonDocument.Parse(line);
                var record = eventDocument.RootElement;
                var kind = record.GetProperty("event").GetString();
                switch (kind)
                {
                    case "init" when payload is null:
                        break;
                    case "step_update" when payload is null:
                        var step = record.GetProperty("step_update");
                        if (step.GetProperty("step_type").GetString() is not ("user_input" or "agent_response" or "thinking" or "finish"))
                            return "";
                        break;
                    case "result" when payload is null:
                        var result = record.GetProperty("result");
                        if (result.GetProperty("status").GetString() != "SUCCESS" ||
                            result.GetProperty("num_turns").GetInt32() != 1)
                            return "";
                        var structured = result.GetProperty("structured_output");
                        if (!HasDocumentaryShape(structured)) return "";
                        payload = structured.GetRawText();
                        break;
                    default:
                        return "";
                }
            }
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException
                                          or FormatException)
        {
            return "";
        }
        return payload ?? "";
    }

    private static bool HasDocumentaryShape(JsonElement root)
    {
        if (!HasFields(root, ("summary", JsonValueKind.String), ("findings", JsonValueKind.Array))) return false;
        foreach (var finding in root.GetProperty("findings").EnumerateArray())
        {
            if (!HasFields(finding, ("category", JsonValueKind.String), ("severity", JsonValueKind.String),
                    ("requirement_id", JsonValueKind.String), ("explanation", JsonValueKind.String),
                    ("suggestion", JsonValueKind.String), ("sources", JsonValueKind.Array)) ||
                finding.GetProperty("severity").GetString() is not ("High" or "Medium" or "Low"))
                return false;

            var sources = finding.GetProperty("sources");
            if (sources.GetArrayLength() == 0) return false;
            foreach (var source in sources.EnumerateArray())
                if (!HasFields(source, ("document_id", JsonValueKind.String), ("content_hash", JsonValueKind.String),
                        ("line_number", JsonValueKind.Number)) ||
                    source.GetProperty("content_hash").GetString() is not { Length: 64 } hash || !hash.All(Uri.IsHexDigit) ||
                    !source.GetProperty("line_number").TryGetInt32(out var line) || line <= 0)
                    return false;
        }
        return true;
    }

    private static bool HasFields(JsonElement element, params (string Name, JsonValueKind Kind)[] fields) =>
        element.ValueKind == JsonValueKind.Object && element.EnumerateObject().Count() == fields.Length &&
        fields.All(field => element.TryGetProperty(field.Name, out var property) && property.ValueKind == field.Kind);
}
