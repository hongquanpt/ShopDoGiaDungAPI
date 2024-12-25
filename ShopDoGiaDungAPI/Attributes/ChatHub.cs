using Microsoft.AspNetCore.SignalR;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;
using System;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    private readonly IMessageService _messageService;

    public ChatHub(IMessageService messageService)
    {
        _messageService = messageService;
    }

    // Khách hàng đăng ký vào group tương ứng với userId của họ
    public async Task RegisterUser(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }

    // Quản trị viên khi muốn trò chuyện với 1 khách hàng cụ thể, join vào group đó (nếu cần)
    public async Task RegisterAdmin(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }

    public async Task SendMessageToAdmin(string user, string message)
    {
        if (!int.TryParse(user, out int userIdInt))
        {
            throw new Exception("UserId is not a valid integer");
        }

        // Thử chỉ send tin mà không lưu DB để kiểm tra
        await Clients.Group(user).SendAsync("ReceiveMessage", user, message);
    }

    // Quản trị viên gửi tin nhắn tới khách hàng
    public async Task SendMessageToUser(string userId, string message)
    {
        // Giả sử adminId = 23 (hoặc bạn có thể lấy từ context)
        var adminId = "23";
        await Clients.Group(userId).SendAsync("ReceiveMessage", adminId, message);
    }

}
