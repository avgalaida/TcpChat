using Server.Models;

namespace Server.Interfaces;

public interface IMessageRepository
{
    Task SaveMessageAsync(ChatMessage message);
    Task<List<ChatMessage>> GetMessagesAsync();
}