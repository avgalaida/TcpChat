namespace Client.Models.Messages;

public class IncomingChatMessage : BaseMessage
{
    public string Sender { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
}