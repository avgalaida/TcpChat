using System.Net;
using Server.Models.Messages;

namespace Server.Services.Client;

/// <summary>
/// Интерфейс для обработки клиентов.
/// </summary>
public interface IClientHandler
{
    /// <summary>
    /// Уникальный идентификатор клиента.
    /// </summary>
    string ClientId { get; }

    /// <summary>
    /// Конечная точка удаленного клиента (IP-адрес и порт).
    /// </summary>
    IPEndPoint RemoteEndPoint { get; }

    /// <summary>
    /// Обрабатывает клиентские сообщения асинхронно.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию обработки сообщений.</returns>
    Task ProcessAsync();

    /// <summary>
    /// Отправляет сообщение клиенту асинхронно.
    /// </summary>
    /// <param name="message">Сообщение для отправки.</param>
    /// <returns>Задача, представляющая асинхронную операцию отправки сообщения.</returns>
    Task SendMessageAsync(BaseMessage message);

    /// <summary>
    /// Отключает клиента.
    /// </summary>
    void Disconnect();
}