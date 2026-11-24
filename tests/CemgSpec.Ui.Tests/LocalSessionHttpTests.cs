using System.Net;
using CemgSpec.Ui.Tests.Fixtures;
namespace CemgSpec.Ui.Tests;
public sealed class LocalSessionHttpTests : IClassFixture<LocalHostFixture>
{
    private readonly LocalHostFixture host;
    public LocalSessionHttpTests(LocalHostFixture host)=>this.host=host;
    [Fact]
    public async Task RejectsForeignHost()
    {
        using var client=host.CreateClient();using var request=new HttpRequestMessage(HttpMethod.Get,"/");request.Headers.Host="attacker.invalid";
        using var response=await client.SendAsync(request); Assert.Equal(HttpStatusCode.MisdirectedRequest,response.StatusCode);
    }
    [Fact]
    public async Task RejectsForeignOrigin()
    {
        using var client=host.CreateClient();using var request=new HttpRequestMessage(HttpMethod.Get,"/");request.Headers.Add("Origin","https://attacker.invalid");
        using var response=await client.SendAsync(request);Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);
    }
    [Fact]
    public async Task IssuesSeparateHttpOnlyStrictSessionCookies()
    {
        using var first=host.CreateClient(false);using var second=host.CreateClient(false);
        using var a=await first.GetAsync("/");using var b=await second.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK,a.StatusCode);
        var ca=Assert.Single(a.Headers.GetValues("Set-Cookie"),x=>x.StartsWith("CemgSpec.Session="));
        var cb=Assert.Single(b.Headers.GetValues("Set-Cookie"),x=>x.StartsWith("CemgSpec.Session="));
        Assert.NotEqual(ca,cb);Assert.Contains("httponly",ca,StringComparison.OrdinalIgnoreCase);Assert.Contains("samesite=strict",ca,StringComparison.OrdinalIgnoreCase);
        Assert.True(a.Headers.CacheControl?.NoStore);
    }
    [Fact]
    public async Task RejectsMutationWithoutAntiforgery()
    {
        using var client=host.CreateClient();using var initial=await client.GetAsync("/");
        using var request=new HttpRequestMessage(HttpMethod.Post,"/"){Content=new StringContent("")};request.Headers.Add("Origin",host.Address.GetLeftPart(UriPartial.Authority));
        using var response=await client.SendAsync(request);Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
    }
    [Fact]
    public async Task RejectsMutationWithoutOrigin()
    {
        using var client=host.CreateClient();using var initial=await client.GetAsync("/");
        using var response=await client.PostAsync("/",new StringContent(""));Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);
    }
    [Fact]
    public async Task RejectsUnissuedSessionOnMutation()
    {
        using var client=host.CreateClient(false);using var request=new HttpRequestMessage(HttpMethod.Post,"/"){Content=new StringContent("")};
        request.Headers.Add("Cookie","CemgSpec.Session=unissued");request.Headers.Add("Origin",host.Address.GetLeftPart(UriPartial.Authority));
        using var response=await client.SendAsync(request);Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode);
    }
    [Fact]
    public async Task AllowsBlazorFallbackTransportWithValidSessionAndExactOrigin()
    {
        var origin = host.Address.GetLeftPart(UriPartial.Authority);
        using var client = host.CreateClient();
        using var initial = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, initial.StatusCode);

        // LongPolling fallback uses POST /_blazor?id=... without HTML form antiforgery tokens,
        // protected by loopback IP, exact Host/Origin and host-issued session cookie.
        using var negotiate = new HttpRequestMessage(HttpMethod.Post, "/_blazor/negotiate?negotiateVersion=1") { Content = new StringContent("") };
        negotiate.Headers.Add("Origin", origin);
        using var negResponse = await client.SendAsync(negotiate);
        Assert.Equal(HttpStatusCode.OK, negResponse.StatusCode);

        using var noOrigin = new HttpRequestMessage(HttpMethod.Post, "/_blazor?id=test") { Content = new StringContent("") };
        using var noOriginResponse = await client.SendAsync(noOrigin);
        Assert.Equal(HttpStatusCode.Forbidden, noOriginResponse.StatusCode);

        using var validTransport = new HttpRequestMessage(HttpMethod.Post, "/_blazor?id=unknown-connection") { Content = new StringContent("") };
        validTransport.Headers.Add("Origin", origin);
        using var transportResponse = await client.SendAsync(validTransport);
        // Must reach SignalR (404 for unknown connection id), NOT 400 AntiforgeryValidationException.
        Assert.NotEqual(HttpStatusCode.BadRequest, transportResponse.StatusCode);
    }
}
