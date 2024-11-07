namespace Server.Models.Messages;

public class HistoryResponse : BaseMessage
{
    public int TotalMessages { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<ChatMessage> Messages { get; set; }
}