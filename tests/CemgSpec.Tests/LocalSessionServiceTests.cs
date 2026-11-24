using Microsoft.AspNetCore.Http;
using CemgSpec.Application;
namespace CemgSpec.Tests;
public sealed class LocalSessionServiceTests
{
    [Fact]
    public void RejectsUnissuedTokenAndForeignSessionOwnership()
    {
        var registry=new LocalSessionRegistry(); var a=registry.Issue(); var b=registry.Issue();
        Assert.False(registry.TryResolve(new string('a',64),out _));
        Assert.True(registry.TryResolve(a.Token,out var id));Assert.Equal(a.Id,id);
        var context=new DefaultHttpContext();context.Request.Headers.Cookie=$"{LocalSessionRegistry.CookieName}={a.Token}";
        var service=new LocalSessionService(new HttpContextAccessor{HttpContext=context},registry);
        Assert.True(service.Owns(a.Id));Assert.False(service.Owns(b.Id));Assert.False(service.Owns(Guid.Empty));
    }
    [Fact]
    public void RefusesServiceWithoutHostIssuedSession()
    {
        Assert.Throws<InvalidOperationException>(()=>new LocalSessionService(new HttpContextAccessor{HttpContext=new DefaultHttpContext()},new LocalSessionRegistry()));
    }
}
