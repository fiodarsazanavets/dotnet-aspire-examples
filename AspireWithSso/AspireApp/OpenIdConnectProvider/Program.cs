using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIdConnectProvider.Data;
using OpenIdConnectProvider.DataSeed;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite($"Filename=identityprovider.sqlite3");
    options.UseOpenIddict();
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

builder.Services.AddOpenIddict()
    .AddCore(coreBuilder =>
    {
        coreBuilder.UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(serverBuilder =>
    {
        serverBuilder.DisableAccessTokenEncryption();
        serverBuilder.SetAuthorizationEndpointUris("connect/authorize")
            .SetEndSessionEndpointUris("connect/logout")
            .SetTokenEndpointUris("connect/token")
            .SetUserInfoEndpointUris("connect/userinfo");
        serverBuilder.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);
        serverBuilder.AllowAuthorizationCodeFlow();
        serverBuilder.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
        serverBuilder.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough()
            .EnableStatusCodePagesIntegration()
            .DisableTransportSecurityRequirement();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddHostedService<InitialDataSeed>();
var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
