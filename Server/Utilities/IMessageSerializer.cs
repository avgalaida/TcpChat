using Server.Models.Messages;

namespace Server.Utilities;

/// <summary>
/// Интерфейс для сериализации и десериализации сообщений.
/// </summary>
public interface IMessageSerializer
{
    /// <summary>
    /// Сериализует сообщение в строку JSON.
    /// </summary>
    /// <param name="message">Сообщение для сериализации.</param>
    /// <returns>Строка JSON, представляющая сериализованное сообщение.</returns>
    string Serialize(BaseMessage message);

    /// <summary>
    /// Десериализует строку JSON в объект сообщения.
    /// </summary>
    /// <param name="messageJson">Строка JSON для десериализации.</param>
    /// <returns>Объект сообщения типа <see cref="BaseMessage"/>.</returns>
    BaseMessage Deserialize(string messageJson);
}