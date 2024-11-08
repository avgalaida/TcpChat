namespace Client.Models.Messages;

/// <summary>
/// Базовый абстрактный класс для всех типов сообщений в системе чата.
/// Содержит общие свойства, которые наследуются всеми конкретными типами сообщений.
/// </summary>
public abstract class BaseMessage
{
    /// <summary>
    /// Тип сообщения, определяющий конкретный тип наследника.
    /// </summary>
    public MessageType Type { get; set; }
}
