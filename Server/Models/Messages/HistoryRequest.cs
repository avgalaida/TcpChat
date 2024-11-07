namespace Server.Models.Messages;

public class HistoryRequest : BaseMessage
{
    public int Page { get; set; }
    public int PageSize { get; set; }
}