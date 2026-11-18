using System.Text.Json;
using SpecAudit.Domain;
using SpecAudit.Infrastructure.Documents;
var root=Path.Combine(Path.GetTempPath(),"SpecAudit-T018-"+Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
var results=new List<object>();
var minimal=Path.Combine(root,"minimal");Directory.CreateDirectory(Path.Combine(minimal,".specify","memory"));
var constitution=Path.Combine(minimal,".specify","memory","constitution.md");File.WriteAllText(constitution,"before");
using(var catalog=new DocumentCatalog(Guid.NewGuid(),minimal))
{
 var recognized=catalog.Entries.Any(e=>e.Kind==CatalogKind.Constitution);
 results.Add(new{id="REV-PRA-01",expected="Existing .specify/memory/constitution.md is catalogued",passed=recognized,fileExists=File.Exists(constitution),catalogCount=catalog.Entries.Count,constitutionRecognized=recognized});
}
var project=Path.Combine(root,"nested");var feature=Path.Combine(project,"specs","001-example");Directory.CreateDirectory(Path.Combine(feature,"evidence"));File.WriteAllText(Path.Combine(feature,"plan.md"),"plan");File.WriteAllText(Path.Combine(feature,"tasks.md"),"tasks");File.WriteAllText(Path.Combine(feature,"evidence","spec.md"),"historical evidence");
using(var catalog=new DocumentCatalog(Guid.NewGuid(),project))
{
 var id=catalog.Features.Single().Key;
 // Exact predicate used by Projects.razor for each required root artifact.
 var shownPresent=catalog.Entries.Any(e=>e.FeatureId==id && Path.GetFileName(e.RelativePath)=="spec.md");
 var actualPresent=File.Exists(Path.Combine(feature,"spec.md"));
 results.Add(new{id="REV-PRA-02",expected="Ausente for missing feature root spec.md",passed=shownPresent==actualPresent,shownPresent,actualPresent});
}
Console.WriteLine(JsonSerializer.Serialize(new{schema_version=1,fixture="__TEMP_ROOT__/SpecAudit-T018-<unique>",results},new JsonSerializerOptions{WriteIndented=true}));
return JsonSerializer.SerializeToElement(results).EnumerateArray().Any(r=>!r.GetProperty("passed").GetBoolean()) ? 1 : 0;
