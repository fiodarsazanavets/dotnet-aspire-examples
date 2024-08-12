using AspireApp.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Extensions.ServiceDiscovery;

namespace AspireApp.Web.Extensions;

public static class AuthExtensions
{
    public static void ConfigureWebAppOpenIdConnect(this AuthenticationBuilder authentication)
    {
        // Named options
        authentication.Services.AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
            .Configure<IConfiguration, IHttpClientFactory, IHostEnvironment>(configure);

        // Unnamed options
        authentication.Services.AddOptions<OpenIdConnectOptions>()
            .Configure<IConfiguration, IHttpClientFactory, IHostEnvironment>(configure);

        static void configure(OpenIdConnectOptions options, IConfiguration configuration, IHttpClientFactory httpClientFactory, IHostEnvironment hostEnvironment)
        {
            var backchannelHttpClient = httpClientFactory.CreateClient(Constants.OidcBackchannel);

            options.Backchannel = backchannelHttpClient;
            options.Authority = backchannelHttpClient.GetIdpAuthorityUri().ToString();
            options.ClientId = "webapp";
            options.ClientSecret = "some_secret";
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.SaveTokens = true;
            options.RequireHttpsMetadata = !hostEnvironment.IsDevelopment();
            options.MapInboundClaims = false;
        }
    }

    public static Uri GetIdpAuthorityUri(this HttpClient httpClient)
    {
        var idpBaseUri = httpClient.BaseAddress
            ?? throw new InvalidOperationException($"HttpClient instance does not have a BaseAddress configured.");
        return new Uri(idpBaseUri, "realms/WeatherApp/");
    }

    public static Uri ResolveIdpAuthorityUri(this ServiceEndpointResolver resolver, string serviceName = "http://idp")
    {
        var idpBaseUrl = resolver.ResolveEndPointUrlAsync(serviceName).AsTask().GetAwaiter().GetResult()
            ?? throw new InvalidOperationException($"Could not resolve IdP address using service name '{serviceName}'.");
        return new Uri(new Uri(idpBaseUrl), "realms/weatherapp/");
    }
}
