namespace Server.Models.Messages;

public abstract class BaseMessage
{
    public MessageType Type { get; set; }
}