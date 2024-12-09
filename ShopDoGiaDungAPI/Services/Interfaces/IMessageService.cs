using ShopDoGiaDungAPI.DTO;
using ShopDoGiaDungAPI.Models;

namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface IMessageService
    {
        Task<IEnumerable<Message>> GetMessagesAsync();
        Task<Message> SaveMessageAsync(Message message);
        Task<IEnumerable<MessageDto>> GetMessagesForUserAsync(int userId, int adminId);
        Task<IEnumerable<MessageDto>> GetMessagesForUserAsync2(int userId, int adminId);
    }
}
