namespace Client.Models;

public class ChatMessage
{
    public string Sender { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsSentByUser { get; set; }
    public string SenderIp { get; set; }
    public int SenderPort { get; set; }
}

