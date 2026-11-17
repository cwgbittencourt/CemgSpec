namespace SpecAudit.Infrastructure.Execution;
public sealed class LimitExceededException(string dimension) : Exception($"LimitExceeded: {dimension}")
{
    public string Result => "Inconclusive";
}
public sealed record ExecutionLimits
{
    public const string PolicyVersion="specaudit-limits-v1";
    public long MaxDocumentBytes { get; init; }=4*1024*1024;
    public int MaxGraphNodes { get; init; }=256;
    public int MaxGraphDepth { get; init; }=32;
    public int MaxOutputCharacters { get; init; }=65536;
    public TimeSpan MaxDuration { get; init; }=TimeSpan.FromSeconds(30);
    public void CheckDocument(long bytes){if(bytes<0 || bytes>MaxDocumentBytes)throw new LimitExceededException("document-bytes");}
    public void CheckGraph(int nodes,int depth){if(nodes<0 || depth<0 || nodes>MaxGraphNodes || depth>MaxGraphDepth)throw new LimitExceededException("evidence-graph");}
    public void CheckOutput(int characters){if(characters<0 || characters>MaxOutputCharacters)throw new LimitExceededException("output-characters");}
    public void CheckDuration(TimeSpan elapsed){if(elapsed<TimeSpan.Zero || elapsed>MaxDuration)throw new LimitExceededException("duration");}
}
