using System.Reflection;
using Microsoft.AspNetCore.Http;
using CemgSpec.Application;
using CemgSpec.Domain;
using CemgSpec.Infrastructure.Reports;

namespace CemgSpec.Tests;

public sealed class BusinessDashboardServiceTests
{
    private sealed class UnusedPicker : IFolderPicker
    {
        public Task<string?> PickAsync(string title) => Task.FromResult<string?>(null);
    }

    private static readonly Assembly ProductAssembly = typeof(ProjectSession).Assembly;

    [Fact]
    public void FR002AndFR003BusinessDashboardContractsExistAndExposeRequiredProjections()
    {
        var rowType = RequireType("CemgSpec.Domain.UserStoryStatusRow");
        var deltaType = RequireType("CemgSpec.Domain.EvolutionDelta");
        var projectionType = RequireType("CemgSpec.Domain.BusinessDashboardProjection");
        var serviceType = RequireType("CemgSpec.Application.BusinessDashboardService");

        Assert.NotNull(rowType);
        Assert.NotNull(deltaType);
        Assert.NotNull(projectionType);
        Assert.NotNull(serviceType);
    }

    [Fact]
    public void FR006ExecutiveSummarySerializerContractExists()
    {
        var serializerType = RequireType("CemgSpec.Infrastructure.Reports.ExecutiveSummarySerializer");
        Assert.NotNull(serializerType);
    }

    [Fact]
    public void HumanValidationDecisionsApplyOnlyToMatchingStoryAndRecognizeRealTemplates()
    {
        var root = Path.Combine(Path.GetTempPath(), "CemgSpec-BA-Rel-" + Guid.NewGuid().ToString("N"));
        var origin = Path.Combine(root, "origin");
        var output = Path.Combine(root, "output");
        var feat = Path.Combine(origin, "specs", "001-demo");
        Directory.CreateDirectory(Path.Combine(feat, "validation"));
        Directory.CreateDirectory(Path.Combine(feat, "acceptance"));
        Directory.CreateDirectory(output);

        File.WriteAllText(Path.Combine(feat, "spec.md"), """
            # Spec
            ### US1 — Envio de mensagem
            ### US2 — Saudação
            ## Requisitos funcionais
            - RF-001: Tela única, coberta por US1-AC01.
            - RF-002: Saudação, coberta por US2-AC01.
            """);
        File.WriteAllText(Path.Combine(feat, "tasks.md"), """
            ## Fase 3 — US1: Envio
            - [x] T001 [US1] (RF-001) Implementar tela.
            ## Fase 4 — US2: Saudação
            - [ ] T002 [US2] (RF-002) Implementar saudação.
            """);
        File.WriteAllText(Path.Combine(feat, "validation", "US1-dev.md"), """
            # Roteiro DEV US1
            Critérios ainda pendentes: texto explicativo.
            Resultados e evidências: APROVADO
            """);
        File.WriteAllText(Path.Combine(feat, "validation", "US1-uat.md"), """
            # Plano UAT US1
            - Decisão para publicação em Produção: APROVADO
            """);
        File.WriteAllText(Path.Combine(feat, "acceptance", "US1.md"), """
            # Aceite US1
            - Decisão: ACEITO
            """);

        try
        {
            var registry = new LocalSessionRegistry();
            var issued = registry.Issue();
            var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            accessor.HttpContext.Items[typeof(LocalSessionService)] = issued.Id;
            var session = new LocalSessionService(accessor, registry);
            using var projectService = new ProjectService(session, new UnusedPicker());
            var ctx = projectService.OpenSelected(origin, output);

            var dashService = new DocumentaryDashboardService();
            var historyService = new SavedHistoryService(projectService, new SavedHistoryReader());
            var baService = new BusinessDashboardService(dashService, historyService);

            var projection = baService.Build(ctx);
            Assert.Equal(2, projection.Stories.Count);

            var us1 = projection.Stories.Single(s => s.StoryId == "US1");
            Assert.Equal("Aceito", us1.DevValidationState);
            Assert.Equal("Aceito", us1.UserValidationState);
            Assert.Equal("Aceito", us1.PoAcceptanceState);
            Assert.NotNull(us1.ValidationDocId);
            Assert.NotNull(us1.UserValidationDocId);
            Assert.NotNull(us1.AcceptanceDocId);

            // US2 MUST remain Pendente across all 3 columns (never inheriting US1-uat.md!)
            var us2 = projection.Stories.Single(s => s.StoryId == "US2");
            Assert.Equal("Pendente", us2.DevValidationState);
            Assert.Equal("Pendente", us2.UserValidationState);
            Assert.Equal("Pendente", us2.PoAcceptanceState);
            Assert.Null(us2.UserValidationDocId);

            Assert.Equal(1, projection.ReadyStories);
            Assert.Equal(1, projection.PendingDevCount);
            Assert.Equal(1, projection.PendingUserCount);
            Assert.Equal(1, projection.PendingPoCount);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static Type RequireType(string fullName)
    {
        var type = ProductAssembly.GetType(fullName, throwOnError: false);
        Assert.True(type is not null, $"Expected contract type '{fullName}' to be implemented in 002 T002-T008.");
        return type!;
    }
}
