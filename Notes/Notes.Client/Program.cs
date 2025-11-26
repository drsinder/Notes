using Blazored.Modal;
using Blazored.SessionStorage;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Notes.Client;
using Notes.Client.Shared;
using Notes.Protos;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddScoped<CookieStateAgent>();  // for login state mgt = "myState" injection in _imports.razor

SyncfusionLicenseProvider
    .RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH5fcnVcRWReWEN0WEpWYEg=");

builder.Services.AddSyncfusionBlazor();   // options => { options.IgnoreScriptIsolation = true; });


// Add my gRPC service so it can be injected.
builder.Services.AddSingleton(services =>
{
    Globals.AppVirtDir = ""; // preset for localhost
    string baseUri = services.GetRequiredService<NavigationManager>().BaseUri;
    string[] parts = baseUri.Split('/');
    if (!baseUri.Contains("localhost")) // not localhost - assume it is in a virtual directory ONLY ONE LEVEL DOWN from root of site
    {
        Globals.AppVirtDir = "/" + parts[^2];
    }

    SubdirectoryHandler? handler = new(new HttpClientHandler(), Globals.AppVirtDir);
    GrpcChannel? channel = GrpcChannel.ForAddress(baseUri,
        new GrpcChannelOptions
        { HttpHandler = new GrpcWebHandler(handler), MaxReceiveMessageSize = 50 * 1024 * 1024 });   // up to 50MB

    NotesServer.NotesServerClient Client = new(channel);
    return Client;
});



await builder.Build().RunAsync();

/// <summary>
/// A delegating handler that adds a subdirectory to the URI of gRPC requests.
/// </summary>
public class SubdirectoryHandler : DelegatingHandler
{
    private readonly string _subdirectory;

    public SubdirectoryHandler(HttpMessageHandler innerHandler, string subdirectory)
        : base(innerHandler)
    {
        _subdirectory = subdirectory;
    }

    
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Uri? old = request.RequestUri;

        string? url = $"{old.Scheme}://{old.Host}:{old.Port}";
        url += $"{_subdirectory}{request.RequestUri.AbsolutePath}";
        request.RequestUri = new Uri(url, UriKind.Absolute);

        Console.WriteLine(request.RequestUri);

        var response = base.SendAsync(request, cancellationToken);

        return response;
    }
    
}
