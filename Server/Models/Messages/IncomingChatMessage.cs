namespace Server.Models.Messages;

/// <summary>
/// Класс, представляющий входящее сообщение чата.
/// </summary>
public class IncomingChatMessage : BaseMessage
{
    /// <summary>
    /// Содержимое сообщения.
    /// </summary>
    public string Content { get; set; }
}