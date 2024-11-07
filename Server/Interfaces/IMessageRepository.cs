using Server.Models.Messages;

namespace Server.Interfaces;

public interface IMessageRepository
{
    Task SaveMessageAsync(ChatMessage message);
    Task<List<ChatMessage>> GetMessagesAsync(int page, int pageSize);
    Task<int> CountAsync();
}