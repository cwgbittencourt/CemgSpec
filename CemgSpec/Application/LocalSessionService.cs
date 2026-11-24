using System.Collections.Concurrent;
using System.Security.Cryptography;
namespace CemgSpec.Application;

// An unissued cookie never creates authority. State lasts only for this host process.
public sealed class LocalSessionRegistry
{
    public const string CookieName = "CemgSpec.Session";
    private readonly ConcurrentDictionary<string, Guid> sessions = new(StringComparer.Ordinal);
    public bool TryResolve(string? token, out Guid id)
    {
        id = default;
        return token is { Length: 64 } && sessions.TryGetValue(token, out id);
    }
    public (string Token, Guid Id) Issue()
    {
        while (true)
        {
            var token=Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
            var id=Guid.NewGuid();
            if(sessions.TryAdd(token,id))return(token,id);
        }
    }
}

public sealed class LocalSessionService
{
    public Guid SessionId { get; }
    public LocalSessionService(IHttpContextAccessor accessor, LocalSessionRegistry registry)
    {
        var context=accessor.HttpContext ?? throw new InvalidOperationException("A host-issued local session is required.");
        if(context.Items.TryGetValue(typeof(LocalSessionService),out var issued) && issued is Guid id)SessionId=id;
        else if(registry.TryResolve(context.Request.Cookies[LocalSessionRegistry.CookieName],out var existing))SessionId=existing;
        else throw new InvalidOperationException("A host-issued local session is required.");
    }
    public bool Owns(Guid ownerSessionId) => ownerSessionId != Guid.Empty && ownerSessionId == SessionId;
}
