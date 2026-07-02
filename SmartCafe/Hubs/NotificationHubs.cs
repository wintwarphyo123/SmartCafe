using Microsoft.AspNetCore.SignalR;
namespace SmartCafe.Hubs
{
    public class NotificationHubs:Hub
    {
        public async Task SendNewOrderAlert(object orderData)
        {
            await Clients.All.SendAsync("newOrderCreated", orderData);
        }

        public async Task UpdateOrderStatus(object orderStatusPayload)
        {
            await Clients.All.SendAsync("orderStatusUpdated", orderStatusPayload);
        }
    }
}
