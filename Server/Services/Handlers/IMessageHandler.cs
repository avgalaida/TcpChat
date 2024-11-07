using Server.Models.Messages;
using Server.Services.Client;

namespace Server.Services.Handlers;

public interface IMessageHandler<TMessage> where TMessage : BaseMessage
{
    Task HandleAsync(TMessage message, IClientHandler clientHandler);
}