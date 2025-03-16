using AspireApp.AppHost.Extensions;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var idp = builder.AddKeycloakContainer("idp", tag: "23.0")
    .ImportRealms("Keycloak")
    .WithExternalHttpEndpoints();

var apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithReference(idp);

var webFrontend = builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(idp, env: "Identity__ClientSecret");

if (builder.Environment.IsDevelopment())
{
    var webAppHttp = webFrontend.GetEndpoint("http");
    var webAppHttps = webFrontend.GetEndpoint("https");

    idp.WithEnvironment("WEBAPP_HTTP", () => $"{webAppHttp.Scheme}://{webAppHttp.Host}:{webAppHttp.Port}");
    
    if (webAppHttps.Exists)
    {
        idp.WithEnvironment("WEBAPP_HTTP_CONTAINERHOST", webAppHttps);
        idp.WithEnvironment("WEBAPP_HTTPS", () => $"{webAppHttps.Scheme}://{webAppHttps.Host}:{webAppHttps.Port}");
    }
    else
    {
        idp.WithEnvironment("WEBAPP_HTTP_CONTAINERHOST", webAppHttp);
    }
}

builder.Build().Run();