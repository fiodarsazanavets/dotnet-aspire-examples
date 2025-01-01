using Microsoft.AspNetCore.SignalR.Client;

namespace FoodDelivery.Web.Extensions;

public static class HubConnectionExtensions
{
    public static IHubConnectionBuilder WithUrl(this IHubConnectionBuilder builder, string url, IHttpMessageHandlerFactory clientFactory)
    {
        return builder.WithUrl(url, options =>
        {
            options.HttpMessageHandlerFactory = _ => clientFactory.CreateHandler();
        });
    }
}