namespace Server.Models.Messages;

public enum MessageType : byte
{
    ChatMessage = 0x01,
    HistoryRequest = 0x02,
    HistoryResponse = 0x03
}