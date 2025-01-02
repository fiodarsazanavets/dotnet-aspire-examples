using Microsoft.AspNetCore.SignalR;

namespace FoodDelivery.ApiService;

public class LocationHub : Hub
{
    public async Task UpdateLocation(double latitude, double longitude)
    {
        await Clients.All.SendAsync("ReceiveLocationUpdate", latitude, longitude);
    }
}