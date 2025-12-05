/*--------------------------------------------------------------------------
    **
    **  Copyright © 2026, Dale Sinder
    **
    **  This program is free software: you can redistribute it and/or modify
    **  it under the terms of the GNU General Public License version 3 as
    **  published by the Free Software Foundation.
    **
    **  This program is distributed in the hope that it will be useful,
    **  but WITHOUT ANY WARRANTY; without even the implied warranty of
    **  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    **  GNU General Public License version 3 for more details.
    **
    **  You should have received a copy of the GNU General Public License
    **  version 3 along with this program in file "license-gpl-3.0.txt".
    **  If not, see <http://www.gnu.org/licenses/gpl-3.0.txt>.
    **
    **--------------------------------------------------------------------------*/

using Blazored.Modal;
using Blazored.SessionStorage;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Notes.Client;
using Notes.Client.Comp;
using Notes.Components;
using Notes.Components.Account;
using Notes.Data;
using Notes.Entities;
using Notes.Protos;
using Notes.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

/// Configure gRPC options for message size limits
builder.Services.AddGrpc()
    .AddServiceOptions<NotesService>(options =>
    {
        options.MaxReceiveMessageSize = 50 * 1024 * 1024; // 50 MB
        options.MaxSendMessageSize = 50 * 1024 * 1024; // 50 MB
                                                       //        options.Interceptors.Add<AuthInterceptor>();
    });

// Configure CORS to allow gRPC-Web calls from any origin
builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddScoped<CookieStateAgent>();  // for login state mgt

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidAudience = builder.Configuration["JWTAuth:ValidAudienceURL"],
            ValidIssuer = builder.Configuration["JWTAuth:ValidIssuerURL"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTAuth:SecretKey"]))
        };
    })
    .AddIdentityCookies();

builder.Services.AddAuthorization();

// Configure Entity Framework and Identity
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<NotesDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<NotesDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Default Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    options.User.RequireUniqueEmail = true;
});

// Add Hangfire services.  Uses same database.
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityEmailSender>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

Globals.SendGridApiKey = builder.Configuration["SendGridApiKey"];
Globals.SendGridEmail = builder.Configuration["SendGridEmail"];
Globals.SendGridName = builder.Configuration["SendGridName"];
Globals.ImportRoot = builder.Configuration["ImportRoot"];
Globals.CookieName = builder.Configuration["CookieName"];
Globals.ImportMailInterval = int.Parse(builder.Configuration["ImportMailInterval"]);
Globals.AppUrl = (builder.Configuration["AppUrl"]);

// Configure gRPC client with GrpcWebHandler and SubdirectoryHandler for virtual directory support
builder.Services.AddSingleton(services =>
{
    string AppVirtDir = ""; // preset for localhost / Development
    string? baseUri = Globals.AppUrl;  //services.GetRequiredService<NavigationManager>().BaseUri;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
    string[] parts = baseUri.Split('/');
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    if (!baseUri.Contains("localhost")) // not localhost - assume it is in a virtual directory ONLY ONE LEVEL DOWN from root of site
    {
        AppVirtDir = "/" + parts[^2];
    }

    SubdirectoryHandler handler = new(new HttpClientHandler(), AppVirtDir);

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

var app = builder.Build();

Globals.HangfireAddress = "/hangfire";

app.UseHangfireDashboard(Globals.HangfireAddress, new DashboardOptions
{
    Authorization = [new Notes.MyAuthorizationFilter()]
});

app.UseGrpcWeb(options: new GrpcWebOptions { DefaultEnabled = true });
app.UseCors();


app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Notes.Client._Imports).Assembly);

app.MapGrpcService<NotesService>()
    .EnableGrpcWeb()
    .RequireCors("AllowAll"); ;


// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();


namespace Notes
{

    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        //[Authorize(Roles = "Admin")]
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}