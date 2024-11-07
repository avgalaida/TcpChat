using Server.Models.Messages;

namespace Server.Repository;

public interface IMessageRepository
{
    Task SaveMessageAsync(OutgoingChatMessage message);
    Task<List<OutgoingChatMessage>> GetMessagesAsync(int page, int pageSize);
    Task<int> CountAsync();
}