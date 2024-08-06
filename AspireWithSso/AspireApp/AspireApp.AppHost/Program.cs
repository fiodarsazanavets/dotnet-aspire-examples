const string WEBAPP_HTTP_ENVIRONMENT_VARIABLE = "WEBAPP_HTTP";
const string IDP_HTTP_ENVIRONMENT_VARIABLE = "IDP_HTTP";

var builder = DistributedApplication.CreateBuilder(args);

var identityProvider = builder.AddProject<Projects.OpenIdConnectProvider>("identityprovider")
    .WithExternalHttpEndpoints();

var apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
	.WithReference(identityProvider);

var webFrontend = builder.AddProject<Projects.AspireApp_Web>("webfrontend")
	.WithExternalHttpEndpoints()
	.WithReference(apiService)
    .WithReference(identityProvider);

var webAppHttp = webFrontend.GetEndpoint("http");
var webAppHttps = webFrontend.GetEndpoint("https");

if (webAppHttps.Exists)
{
    identityProvider.WithEnvironment(WEBAPP_HTTP_ENVIRONMENT_VARIABLE, () => $"{webAppHttps.Scheme}://{webAppHttps.Host}:{webAppHttps.Port}");
}
else
{
    identityProvider.WithEnvironment(WEBAPP_HTTP_ENVIRONMENT_VARIABLE, () => $"{webAppHttp.Scheme}://{webAppHttp.Host}:{webAppHttp.Port}");
}

var idpAppHttp = identityProvider.GetEndpoint("http");
var idpAppHttps = identityProvider.GetEndpoint("https");

if (idpAppHttps.Exists)
{
    webFrontend.WithEnvironment(IDP_HTTP_ENVIRONMENT_VARIABLE, () => $"{idpAppHttps.Scheme}://{idpAppHttps.Host}:{idpAppHttps.Port}");
    apiService.WithEnvironment(IDP_HTTP_ENVIRONMENT_VARIABLE, () => $"{idpAppHttps.Scheme}://{idpAppHttps.Host}:{idpAppHttps.Port}");
}
else
{
    webFrontend.WithEnvironment(IDP_HTTP_ENVIRONMENT_VARIABLE, () => $"{idpAppHttp.Scheme}://{idpAppHttp.Host}:{idpAppHttp.Port}");
    apiService.WithEnvironment(IDP_HTTP_ENVIRONMENT_VARIABLE, () => $"{idpAppHttp.Scheme}://{idpAppHttp.Host}:{idpAppHttp.Port}");
}

builder.Build().Run();