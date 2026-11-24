using System.IO.Pipes;
using System.Security.Cryptography;
namespace CemgSpec.Infrastructure.Execution;
public sealed class ExecutionChannel : IDisposable
{
    public Guid RunId { get; }
    public long CatalogGeneration { get; }
    public string Token { get; }=Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
    public AnonymousPipeServerStream ToWorker { get; }=new(PipeDirection.Out,HandleInheritability.Inheritable);
    public AnonymousPipeServerStream FromWorker { get; }=new(PipeDirection.In,HandleInheritability.Inheritable);
    private int disposed;
    public ExecutionChannel(Guid run,long generation)
    {
        if(run==Guid.Empty || generation<0)throw new ArgumentException("Run identity and generation are required.");
        RunId=run;CatalogGeneration=generation;
    }
    public bool Authorize(Guid run,string token,long generation)
    {
        if(Volatile.Read(ref disposed)!=0 || run!=RunId || generation!=CatalogGeneration || token.Length!=64)return false;
        try {return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(token),Convert.FromHexString(Token));}
        catch(FormatException){return false;}
    }
    public void Dispose()
    {
        if(Interlocked.Exchange(ref disposed,1)!=0)return;
        ToWorker.Dispose();FromWorker.Dispose();
    }
}
