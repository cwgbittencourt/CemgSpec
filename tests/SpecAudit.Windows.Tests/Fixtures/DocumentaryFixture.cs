using System.Security.Cryptography;
using System.Text.Json;
namespace SpecAudit.Windows.Tests.Fixtures;

// Development data only. Never invoked by the product and never deletes evidence.
public sealed class DocumentaryFixture
{
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "SpecAudit-V1-" + Guid.NewGuid().ToString("N"));
    public string Origin => Path.Combine(Root, "origin");
    public string Output => Path.Combine(Root, "reports");
    public string Evidence => Path.Combine(Root, "dev-evidence");
    public string Feature => Path.Combine(Origin, "specs", "001-demo");
    public Dictionary<string, string> Baseline { get; private set; } = [];
    public static DocumentaryFixture Create()
    {
        var f = new DocumentaryFixture();
        Directory.CreateDirectory(f.Output); Directory.CreateDirectory(f.Evidence);
        f.Write(".specify/memory/constitution.md", "# Demo constitution: source is read-only.");
        var requirements = Enumerable.Range(1,20).Select(i => $"- FR-{i:D3}: Documentary requirement {i}.");
        var tasks = Enumerable.Range(1,40).Select(i => $"- [{(i <= 30 ? "x" : " ")}] T{i:D3} Demo task {i} {(i <= 12 ? $"FR-{i:D3}" : "unlinked")}");
        f.Write("specs/001-demo/spec.md", "# Demo feature\n" + string.Join('\n',requirements));
        f.Write("specs/001-demo/plan.md", "# Demo plan");
        f.Write("specs/001-demo/tasks.md", "# Tasks\n" + string.Join('\n',tasks));
        f.Write("specs/002-other/spec.md", "# Second feature\n- FR-001: Independent identity.");
        f.Write("specs/002-other/plan.md", "# Second plan");
        f.Write("specs/002-other/tasks.md", "# Second tasks\n- [ ] T001 FR-001");
        f.Write("DoNotRead.cs", "// synthetic canary; never read by product");
        f.Write("specs/001-demo/research.md", "# Preview corpus\n\n## Anchor\n\n- [ ] Read-only task\n\n| A | B |\n|---|---|\n| one | two |\n\n> Quote\n[Plan](plan.md)\n[Missing](absent.md)\n[Forbidden](../../DoNotRead.cs)\n<script>window.__unsafe=1</script>\n![remote](http://127.0.0.1:9/controlled.png)\n```mermaid\nflowchart LR\nA-->B\n```\n```mermaid\nINVALID\n```\n```plantuml\nA -> B\n```\n");
        var hash = Hash(Path.Combine(f.Feature,"spec.md"));
        var checks = new Dictionary<string,bool> { ["criteria_observable"]=true,["scenarios_defined"]=true,["scope_defined"]=true,["dependencies_identified"]=true,["material_decisions_resolved"]=true };
        foreach (var variant in new[]{"ready","blocked","bad-hash","wrong-revision","code-reference"})
        {
            var record = new { schema_version=1, stage="spec", revision="sha256:"+(variant=="wrong-revision" ? new string('0',64) : hash), state=variant=="blocked" ? "BLOCKED" : "READY", checks, blockers=variant=="blocked" ? new[]{"Synthetic blocker"} : [], human_decisions=Array.Empty<object>(), evidence=new[]{new{path=variant=="code-reference" ? "../../DoNotRead.cs" : "spec.md",kind="document",sha256=variant=="bad-hash" ? new string('0',64) : hash}} };
            f.Write($"specs/001-demo/verification/{variant}.json",JsonSerializer.Serialize(record));
        }
        f.Write("specs/001-demo/verification/nested-log.json", JsonSerializer.Serialize(new { stdout = "../../DoNotRead.cs", stderr = "" }));
        f.Write("specs/001-demo/verification/outside-artifact.json", JsonSerializer.Serialize(new { artifacts = new[]{new {path="../../../outside.md",sha256=new string('0',64)}} }));
        f.CaptureBaseline();
        return f;
    }
    private void Write(string relative, string content)
    {
        var path=Path.Combine(Origin,relative); Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllText(path,content);
    }
    public void CaptureBaseline()
    {
        Baseline=Directory.EnumerateFiles(Origin,"*",SearchOption.AllDirectories).ToDictionary(p=>Path.GetRelativePath(Origin,p),Hash);
        File.WriteAllText(Path.Combine(Evidence,"baseline.json"),JsonSerializer.Serialize(Baseline));
        File.WriteAllText(Path.Combine(Evidence,"paths.json"),JsonSerializer.Serialize(new{Root,Origin,Output,Evidence,Feature}));
    }
    public bool IsUnchanged() => Baseline.OrderBy(p=>p.Key).SequenceEqual(Directory.EnumerateFiles(Origin,"*",SearchOption.AllDirectories).ToDictionary(p=>Path.GetRelativePath(Origin,p),Hash).OrderBy(p=>p.Key));
    public static string Hash(string path) => Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)));
    public void RecordAccess(string path,string operation,string result) => File.AppendAllText(Path.Combine(Evidence,"access.jsonl"),JsonSerializer.Serialize(new{timestamp=DateTimeOffset.UtcNow,path,operation,result})+"\n");
    // Use barriers to pause a reader exactly between preparation and actual open.
    public Barrier RaceBarrier { get; } = new(2);
}
