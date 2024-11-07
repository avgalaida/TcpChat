namespace Server.Models.Messages;

/// <summary>
/// Базовый класс для всех сообщений.
/// </summary>
public abstract class BaseMessage
{
    /// <summary>
    /// Тип сообщения.
    /// </summary>
    public MessageType Type { get; set; }
}