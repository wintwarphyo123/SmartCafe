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
        public async Task UpdateCategoryStatus(Object categoryUpdateInfo)
        {
            await Clients.All.SendAsync("ReceiveCategoryUpdate", categoryUpdateInfo);
        }
        public async Task UpdateMenuStatus(Object menuUpdateInfo)
        {
            await Clients.All.SendAsync("ReceiveMenuUpdate", menuUpdateInfo);
        }
    }
}
