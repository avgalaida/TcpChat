namespace Client.Models.Messages;

/// <summary>
/// Представляет исходящее сообщение чата, отправленное текущим пользователем.
/// Наследуется от <see cref="BaseMessage"/>.
/// </summary>
public class OutgoingChatMessage : BaseMessage
{
    /// <summary>
    /// Содержимое исходящего сообщения.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="OutgoingChatMessage"/>.
    /// </summary>
    public OutgoingChatMessage()
    {
        Type = MessageType.ChatMessage;
    }
}