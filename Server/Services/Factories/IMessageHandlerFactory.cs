using Server.Models.Messages;
using Server.Services.Client;

namespace Server.Services.Factories;

public interface IMessageHandlerFactory
{
    Task HandleMessageAsync(BaseMessage message, IClientHandler clientHandler);
}