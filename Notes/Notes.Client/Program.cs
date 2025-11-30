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
    string AppVirtDir = ""; // preset for localhost / Development
    string baseUri = services.GetRequiredService<NavigationManager>().BaseUri;
    string[] parts = baseUri.Split('/');
    if (!baseUri.Contains("localhost")) // not localhost - assume it is in a virtual directory ONLY ONE LEVEL DOWN from root of site
    {
        AppVirtDir = "/" + parts[^2];
    }

    SubdirectoryHandler handler = new SubdirectoryHandler(new HttpClientHandler(), AppVirtDir);

    var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, handler))
    {
        BaseAddress = new Uri(baseUri)
    };

    var channel = GrpcChannel.ForAddress(httpClient.BaseAddress, 
        new GrpcChannelOptions { 
            HttpClient = httpClient, 
            MaxReceiveMessageSize = 50 * 1024 * 1024,
            MaxSendMessageSize = 50 * 1024 * 1024
        });

    NotesServer.NotesServerClient Client = new(channel);
    return Client;
});

/*
builder.Services.AddGrpcClient<NotesServer.NotesServerClient>(options =>
    {
        options.Address = new Uri("https://localhost:7093");
    })
    .ConfigurePrimaryHttpMessageHandler(
        () => new GrpcWebHandler(new HttpClientHandler()));
*/

await builder.Build().RunAsync();

namespace Notes.Client
{
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
            Uri? old = request.RequestUri ?? throw new InvalidOperationException("RequestUri cannot be null.");
            string url = $"{old.Scheme}://{old.Host}:{old.Port}";
            url += $"{_subdirectory}{old.AbsolutePath}";
            request.RequestUri = new Uri(url, UriKind.Absolute);

            Console.WriteLine(request.RequestUri);

            var response = base.SendAsync(request, cancellationToken);

            return response;
        }
    }
}
