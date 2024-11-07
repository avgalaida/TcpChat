namespace Server.Models;

public class HistoryRequest : BaseMessage
{
    public int Page { get; set; }
    public int PageSize { get; set; }
}