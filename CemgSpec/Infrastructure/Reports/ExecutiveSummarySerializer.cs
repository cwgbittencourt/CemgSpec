using System.Net;
using System.Text;
using CemgSpec.Application;
using CemgSpec.Domain;
using CemgSpec.Infrastructure.Documents;

namespace CemgSpec.Infrastructure.Reports;

public sealed record ExecutiveExportResult(bool Success, string? FilePath, string Message);

public sealed class ExecutiveSummarySerializer
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public string SerializeHtml(BusinessDashboardProjection projection, DateTimeOffset generatedUtc)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"pt-BR\">");
        sb.AppendLine("<head><meta charset=\"utf-8\"><title>Resumo Executivo — CemgSpec</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;max-width:960px;margin:28px auto;color:#182030;line-height:1.5}h1{margin-bottom:4px}.muted{color:#526078;font-size:13px}.kpis{display:grid;grid-template-columns:repeat(4,1fr);gap:14px;margin:20px 0}.kpi{border:1px solid #cfd8e6;border-radius:8px;padding:14px}.kpi strong{font-size:26px;display:block}table{border-collapse:collapse;width:100%;margin-top:16px}th,td{border-bottom:1px solid #cfd8e6;padding:10px;text-align:left;font-size:14px}th{background:#f3f6fb}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine($"<h1>Resumo Executivo — {WebUtility.HtmlEncode(projection.ProjectDisplayName)}</h1>");
        sb.AppendLine($"<p class=\"muted\">Escopo: {WebUtility.HtmlEncode(projection.ScopeLabel)} · Gerado em {generatedUtc.ToLocalTime():dd/MM/yyyy HH:mm} · Auditoria exclusivamente documental (origem somente leitura).</p>");
        sb.AppendLine("<div class=\"kpis\">");
        sb.AppendLine($"<div class=\"kpi\"><span class=\"muted\">Histórias prontas</span><strong>{projection.ReadyStories} / {projection.ActiveStories}</strong></div>");
        sb.AppendLine($"<div class=\"kpi\"><span class=\"muted\">Aguardando validação DEV</span><strong>{projection.PendingDevCount}</strong></div>");
        sb.AppendLine($"<div class=\"kpi\"><span class=\"muted\">Aguardando validação Usuário</span><strong>{projection.PendingUserCount}</strong></div>");
        sb.AppendLine($"<div class=\"kpi\"><span class=\"muted\">Aguardando aceite PO</span><strong>{projection.PendingPoCount}</strong></div>");
        sb.AppendLine("</div>");
        sb.AppendLine($"<p><strong>Saúde documental:</strong> {projection.ActiveBlockersCount} impedimento(s) ativo(s) · {projection.OpenGapsCount} lacuna(s) aberta(s) · {projection.ReadyGatesCount} gate(s) READY.</p>");
        sb.AppendLine("<h2>Entregas por História de Usuário</h2>");
        sb.AppendLine("<table><thead><tr><th>US</th><th>História</th><th>Tasks</th><th>Validação DEV</th><th>Validação Usuário</th><th>Aceite PO</th></tr></thead><tbody>");
        foreach (var row in projection.Stories)
        {
            var tasksText = row.IsDeferred ? "Adiada" : $"{row.CompletedTasks}/{row.TotalTasks} ({row.ProgressPercentage:0.#}%)";
            sb.AppendLine($"<tr><td><strong>{WebUtility.HtmlEncode(row.StoryId)}</strong></td><td>{WebUtility.HtmlEncode(row.Title)}</td><td>{WebUtility.HtmlEncode(tasksText)}</td><td>{WebUtility.HtmlEncode(row.DevValidationState)}</td><td>{WebUtility.HtmlEncode(row.UserValidationState)}</td><td>{WebUtility.HtmlEncode(row.PoAcceptanceState)}</td></tr>");
        }
        sb.AppendLine("</tbody></table>");
        sb.AppendLine("<p class=\"muted\" style=\"margin-top:24px\">Aviso: indicadores documentais não substituem testes funcionais nem autenticação humana de aprovação.</p>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    public ExecutiveExportResult ExportToExternalDirectory(OpenProjectContext context, BusinessDashboardProjection projection)
    {
        try
        {
            using var originHandle = WindowsReadHandle.Open(context.Catalog.FinalOriginPath, directory: true);
            var finalOrigin = WindowsReadHandle.FinalPath(originHandle);

            var candidateDest = Path.GetFullPath(context.OutputDisplayPath);
            if (ExternalReportWriter.HasReparsePointOnPath(candidateDest))
                return new(false, null, "Destino externo contém reparse point; exportação recusada.");

            using var destHandle = WindowsReadHandle.Open(candidateDest, directory: true);
            var finalDest = WindowsReadHandle.FinalPath(destHandle);
            var destIdentity = WindowsReadHandle.Info(destHandle).Identity;

            if (string.Equals(finalOrigin, finalDest, StringComparison.OrdinalIgnoreCase) ||
                WindowsReadHandle.Contains(finalOrigin, finalDest))
                return new(false, null, "Destino externo não pode estar dentro da origem auditada.");

            if (!string.Equals(destIdentity, context.Session.OutputDirectoryIdentity, StringComparison.Ordinal))
                return new(false, null, "A identidade do destino externo mudou; reconfirme o destino antes de exportar.");

            var now = DateTimeOffset.UtcNow;
            var fileName = $"CemgSpec-Resumo-Executivo-{now:yyyyMMddTHHmmssZ}-{Guid.NewGuid().ToString("N")[..6]}.html";
            var fullPath = Path.Combine(finalDest, fileName);
            var bytes = Utf8NoBom.GetBytes(SerializeHtml(projection, now));
            using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                fs.Write(bytes, 0, bytes.Length);

            return new(true, fullPath, $"Resumo Executivo exportado em {fullPath}.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DocumentAccessException)
        {
            return new(false, null, $"Falha ao exportar Resumo Executivo: {ex.Message}");
        }
    }
}
