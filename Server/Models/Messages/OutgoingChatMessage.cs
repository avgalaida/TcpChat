namespace Server.Models.Messages;

public class OutgoingChatMessage : BaseMessage
{
    public Guid Id { get; set; }
    public string Sender { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
    public string SenderIp { get; set; }
    public int SenderPort { get; set; }
}