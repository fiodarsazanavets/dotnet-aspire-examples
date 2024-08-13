using AspireApp.ServiceDefaults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AspireApp.ApiService.Extensions;

public static class AuthExtensions
{
    public static void ConfigureApiJwt(this AuthenticationBuilder authentication)
    {
        // Named options
        authentication.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IConfiguration, IHttpClientFactory, IHostEnvironment>(configure);

        // Unnamed options
        authentication.Services.AddOptions<JwtBearerOptions>()
            .Configure<IConfiguration, IHttpClientFactory, IHostEnvironment>(configure);

        static void configure(JwtBearerOptions options, IConfiguration configuration, IHttpClientFactory httpClientFactory, IHostEnvironment hostEnvironment)
        {
            var backchannelHttpClient = httpClientFactory.CreateClient(Constants.OidcBackchannel);

            options.Backchannel = backchannelHttpClient;
            options.Authority = backchannelHttpClient.GetIdpAuthorityUri().ToString();
            options.RequireHttpsMetadata = !hostEnvironment.IsDevelopment();
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false
            };
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
