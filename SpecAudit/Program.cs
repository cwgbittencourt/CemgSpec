using System.Net;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using SpecAudit.Application;
using SpecAudit.Components;

var builder = WebApplication.CreateBuilder(args);
var configuredUrls=(builder.Configuration["urls"] ?? "http://127.0.0.1:5187")
    .Split(';',StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
if(configuredUrls.Length==0)throw new InvalidOperationException("A loopback URL is required.");
var origins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
foreach(var value in configuredUrls)
{
    if(!Uri.TryCreate(value,UriKind.Absolute,out var uri) || uri.Scheme is not ("http" or "https") ||
       !uri.IsLoopback || uri.Host is not ("localhost" or "127.0.0.1" or "[::1]" or "::1") ||
       uri.AbsolutePath!="/" || uri.Query.Length!=0 || uri.Fragment.Length!=0 || uri.UserInfo.Length!=0 || uri.Port==0)
        throw new InvalidOperationException("SpecAudit only supports explicit local loopback listeners.");
    origins.Add(uri.GetLeftPart(UriPartial.Authority));
}
builder.WebHost.UseUrls(configuredUrls);
// Do not allow secondary Kestrel configuration to expose the host on the LAN.
if(builder.Configuration.GetSection("Kestrel:Endpoints").GetChildren().Any())
    throw new InvalidOperationException("Configure only explicit loopback URLs; Kestrel endpoint overrides are unsupported.");
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<LocalSessionRegistry>();
builder.Services.AddScoped<LocalSessionService>();
builder.Services.AddScoped<IFolderPicker,SpecAudit.Infrastructure.Documents.NativeFolderPicker>();
var gatePackage = new SpecAudit.Infrastructure.Gates.GatePackage();
var capabilityRegistry = new CapabilityRegistry();
gatePackage.TryRegisterVerifiedSeal(capabilityRegistry);
builder.Services.AddSingleton(capabilityRegistry);
builder.Services.AddSingleton(gatePackage);
builder.Services.AddSingleton<SpecAudit.Infrastructure.Gates.GateExecutor>();
builder.Services.AddSingleton<SpecAudit.Infrastructure.Agents.AgentDiscovery>();
builder.Services.AddSingleton<SpecAudit.Infrastructure.Agents.CodexAnalyzeAdapter>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<DocumentaryDashboardService>();
builder.Services.AddScoped<RequirementTraceService>();
builder.Services.AddScoped<VerificationService>();
builder.Services.AddAntiforgery(options=>
{
    options.Cookie.SameSite=SameSiteMode.Strict;
    options.Cookie.HttpOnly=true;
});
builder.Services.AddRazorComponents().AddInteractiveServerComponents(options=>options.DetailedErrors=false);
var app=builder.Build();

app.Use(async(context,next)=>
{
    var authority=$"{context.Request.Scheme}://{context.Request.Host}";
    if(context.Connection.RemoteIpAddress is not { } remote || !IPAddress.IsLoopback(remote) || !origins.Contains(authority))
    {context.Response.StatusCode=StatusCodes.Status421MisdirectedRequest;return;}
    var origin=context.Request.Headers.Origin;
    var safe=HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method) || HttpMethods.IsOptions(context.Request.Method);
    if(origin.Count>1 || (origin.Count==1 && !string.Equals(origin[0],authority,StringComparison.OrdinalIgnoreCase)) ||
       (!safe && origin.Count!=1) || context.Request.Headers["Sec-Fetch-Site"]=="cross-site")
    {context.Response.StatusCode=StatusCodes.Status403Forbidden;return;}
    var registry=context.RequestServices.GetRequiredService<LocalSessionRegistry>();
    if(!registry.TryResolve(context.Request.Cookies[LocalSessionRegistry.CookieName],out var sessionId))
    {
        if(!safe){context.Response.StatusCode=StatusCodes.Status401Unauthorized;return;}
        var issued=registry.Issue();sessionId=issued.Id;
        context.Response.Cookies.Append(LocalSessionRegistry.CookieName,issued.Token,new CookieOptions{HttpOnly=true,SameSite=SameSiteMode.Strict,Secure=context.Request.IsHttps,IsEssential=true,Path="/"});
    }
    context.Items[typeof(LocalSessionService)]=sessionId;
    context.Response.Headers.CacheControl="no-store";
    context.Response.Headers.XContentTypeOptions="nosniff";
    context.Response.Headers["Content-Security-Policy"]="frame-ancestors 'none'";
    // SignalR transport endpoints (negotiate, LongPolling/SSE messages, disconnect) do not carry
    // HTML form antiforgery tokens. They remain protected by loopback IP, exact Host/Origin,
    // Sec-Fetch-Site and the existing host-issued HttpOnly SameSite=Strict session above.
    var isBlazorTransport = context.Request.Path == "/_blazor"
        || context.Request.Path == "/_blazor/negotiate"
        || context.Request.Path == "/_blazor/disconnect";
    if(!safe && !isBlazorTransport)
    {
        try {await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context);}
        catch(AntiforgeryValidationException){context.Response.StatusCode=StatusCodes.Status400BadRequest;return;}
    }
    await next();
});
if(!app.Environment.IsDevelopment())app.UseExceptionHandler("/Error",createScopeForErrors:true);
app.UseStatusCodePagesWithReExecute("/not-found",createScopeForStatusCodePages:true);
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
