using AspireApp.AppHost.Extensions;

const string WEBAPP_HTTP_CONTAINERHOST_ENV_VARIABLE = "WEBAPP_HTTP_CONTAINERHOST";
const string WEBAPP_HTTP_ENV_VARIABLE = "WEBAPP_HTTP";
const string WEBAPP_HTTPS_ENV_VARIABLE = "WEBAPP_HTTPS";
const string IDP_HTTP_ENVIRONMENT_VARIABLE = "IDP_HTTP";

var builder = DistributedApplication.CreateBuilder(args);

var idp = builder.AddKeycloakContainer("idp", tag: "23.0")
    .ImportRealms("Keycloak")
    .WithExternalHttpEndpoints();

var apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithReference(idp, env: "Identity__ClientSecret");

var webFrontend = builder.AddProject<Projects.AspireApp_Web>("webfrontend")
	.WithExternalHttpEndpoints()
	.WithReference(apiService)
    .WithReference(idp, env: "Identity__ClientSecret");

var webAppHttp = webFrontend.GetEndpoint("http");
var webAppHttps = webFrontend.GetEndpoint("https");

idp.WithEnvironment(WEBAPP_HTTP_CONTAINERHOST_ENV_VARIABLE, webAppHttp);
idp.WithEnvironment(WEBAPP_HTTP_ENV_VARIABLE, () => $"{webAppHttp.Scheme}://{webAppHttp.Host}:{webAppHttp.Port}");
if (webAppHttps.Exists)
{
    idp.WithEnvironment(WEBAPP_HTTP_CONTAINERHOST_ENV_VARIABLE, webAppHttps);
    idp.WithEnvironment(WEBAPP_HTTPS_ENV_VARIABLE, () => $"{webAppHttps.Scheme}://{webAppHttps.Host}:{webAppHttps.Port}");
}
else
{
    idp.WithEnvironment(WEBAPP_HTTP_CONTAINERHOST_ENV_VARIABLE, webAppHttp);
    idp.WithEnvironment(WEBAPP_HTTP_ENV_VARIABLE, () => $"{webAppHttp.Scheme}://{webAppHttp.Host}:{webAppHttp.Port}");
}

builder.Build().Run();