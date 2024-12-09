using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IUserService _userService;
    public ChatController(IMessageService messageService, IUserService userService)
    {
        _messageService = messageService;
        _userService = userService;
    }

    // Lấy tất cả tin nhắn giữa 1 khách hàng và admin (adminId=2)
    [HttpGet("messages/{userId}")]
    public async Task<IActionResult> GetMessagesForUser(int userId)
    {
        var messages = await _messageService.GetMessagesForUserAsync(userId, 23);
        return Ok(messages);
    }
    [HttpGet("messages2/{userId}")]
    public async Task<IActionResult> GetMessagesForUser2(int userId)
    {
        var messages = await _messageService.GetMessagesForUserAsync2(userId, 23);
        return Ok(messages);
    }
    // Gửi tin nhắn (từ user->admin hoặc admin->user)
    // userId ở đây là người dùng, message trong body chứa SenderId, ReceiverId, Content
    [HttpPost("{userId}/send")]
    public async Task<IActionResult> SendMessage(int userId, [FromBody] Message message)
    {
        var savedMessage = await _messageService.SaveMessageAsync(message);
        return Ok(savedMessage);
    }
    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }
}
