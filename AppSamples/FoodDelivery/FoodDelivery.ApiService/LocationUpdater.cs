using Microsoft.AspNetCore.SignalR;

namespace FoodDelivery.ApiService;

public class LocationUpdater(IHubContext<LocationHub> locationHub) : BackgroundService
{
    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(5000, cancellationToken);
        await locationHub.Clients.All.SendAsync("ReceiveLocationUpdate", 51.5074, -0.1276, cancellationToken);

        await Task.Delay(5000, cancellationToken);
        await locationHub.Clients.All.SendAsync("ReceiveLocationUpdate", 51.5074, -0.13, cancellationToken);

        await Task.Delay(5000, cancellationToken);
        await locationHub.Clients.All.SendAsync("ReceiveLocationUpdate", 51.508, -0.14, cancellationToken);
    }
}
