using Microsoft.AspNetCore.SignalR;
using ShopDoGiaDungAPI.Services.Interfaces;
namespace ShopDoGiaDungAPI.Services.Implementations
{
    public class OrderNotificationService : IOrderNotificationService
    {
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderNotificationService(IHubContext<OrderHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyNewOrderAsync(object orderData)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNewOrder", orderData);
        }

        public async Task NotifyOrderUpdatedAsync(object orderData)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdated", orderData);
        }
    }
}
