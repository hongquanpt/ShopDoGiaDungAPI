using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class OrderHub : Hub
{
    public async Task RegisterAdmin(string adminId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, adminId);
    }

    // Sự kiện khi có đơn hàng mới
    public async Task SendNewOrder(object newOrder)
    {
        await Clients.All.SendAsync("ReceiveNewOrder", newOrder);
    }

    // Sự kiện khi đơn hàng được cập nhật (thay đổi trạng thái)
    public async Task SendOrderUpdated(object updatedOrder)
    {
        await Clients.All.SendAsync("ReceiveOrderUpdated", updatedOrder);
    }
}
