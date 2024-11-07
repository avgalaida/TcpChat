using System;
using System.Net;
using System.Threading.Tasks;
using Client.Models.Messages;

namespace Client.Services;

/// <summary>
/// Интерфейс для реализации сервиса чата для клиентского WPF-приложения.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Событие, возникающее при получении сообщения от сервера.
    /// </summary>
    event EventHandler<ServerChatMessage> MessageReceived;

    /// <summary>
    /// Событие, возникающее при получении истории сообщений.
    /// </summary>
    event EventHandler<HistoryResponse> HistoryReceived;

    /// <summary>
    /// Событие, возникающее при возникновении ошибки.
    /// </summary>
    event EventHandler<string> OnError;

    /// <summary>
    /// Асинхронно подключается к серверу чата.
    /// </summary>
    /// <returns>Задача, представляющая результат подключения (успех или неудача).</returns>
    Task<bool> ConnectAsync();

    /// <summary>
    /// Асинхронно отключается от сервера чата.
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Асинхронно отправляет сообщение чата на сервер.
    /// </summary>
    /// <param name="content">Содержимое сообщения для отправки.</param>
    Task SendChatMessageAsync(string content);

    /// <summary>
    /// Асинхронно отправляет запрос на получение истории сообщений.
    /// </summary>
    /// <param name="page">Номер страницы.</param>
    /// <param name="pageSize">Размер страницы (количество сообщений).</param>
    Task RequestHistoryAsync(int page, int pageSize);

    /// <summary>
    /// Возвращает локальную конечную точку подключения клиента.
    /// </summary>
    IPEndPoint LocalEndPoint { get; }

    /// <summary>
    /// Возвращает текущее состояние подключения.
    /// </summary>
    bool IsConnected { get; }
}