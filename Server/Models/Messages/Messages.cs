namespace Server.Models.Messages;
public abstract class BaseMessage
{
    public MessageType Type { get; set; }
}

public enum MessageType
{
    ChatMessage,
    HistoryRequest,
    HistoryResponse
}

public class IncomingChatMessage : BaseMessage
{
    public string Content { get; set; }
}

public class OutgoingChatMessage : BaseMessage
{
    public Guid Id { get; set; }
    public string Sender { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
    public string SenderIp { get; set; }
    public int SenderPort { get; set; }
}

public class HistoryRequest : BaseMessage
{
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class HistoryResponse : BaseMessage
{
    public int TotalMessages { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<OutgoingChatMessage> Messages { get; set; }
}
