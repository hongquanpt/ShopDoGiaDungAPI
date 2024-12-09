using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MessageService : IMessageService
{
    private readonly OnlineShopContext _context;

    public MessageService(OnlineShopContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Message>> GetMessagesAsync()
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesForUserAsync(int userId, int adminId)
    {
        return await _context.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == 23)|| (m.SenderId == 23 && m.ReceiverId == userId))
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();
    }
    public async Task<IEnumerable<MessageDto>> GetMessagesForUserAsync2(int userId, int adminId)
    {
        return await _context.Messages
            .Where(m => (m.SenderId == 23 && m.ReceiverId == userId))
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();
    }
    public async Task<Message> SaveMessageAsync(Message message)
    {
        try
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }
        catch (Exception ex)
        {
            // log ex.Message
            throw;
        }
    }
}
