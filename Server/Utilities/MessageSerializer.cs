using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Server.Models.Messages;
using Microsoft.Extensions.Logging;

namespace Server.Utilities;

/// <summary>
/// Класс для сериализации и десериализации сообщений.
/// </summary>
public class MessageSerializer : IMessageSerializer
{
    private readonly ILogger<MessageSerializer> _logger;
    private readonly JsonSerializerSettings _serializerSettings;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MessageSerializer"/>.
    /// </summary>
    /// <param name="logger">Экземпляр логгера для записи логов.</param>
    public MessageSerializer(ILogger<MessageSerializer> logger)
    {
        _logger = logger;
        _serializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new StringEnumConverter() },
            NullValueHandling = NullValueHandling.Ignore,
            StringEscapeHandling = StringEscapeHandling.Default,
            Formatting = Formatting.None
        };
    }

    /// <summary>
    /// Сериализует сообщение в строку JSON.
    /// </summary>
    /// <param name="message">Сообщение для сериализации.</param>
    /// <returns>Строка JSON, представляющая сериализованное сообщение.</returns>
    public string Serialize(BaseMessage message)
    {
        return JsonConvert.SerializeObject(message, _serializerSettings);
    }

    /// <summary>
    /// Десериализует строку JSON в объект сообщения.
    /// </summary>
    /// <param name="messageJson">Строка JSON для десериализации.</param>
    /// <returns>Объект сообщения типа <see cref="BaseMessage"/>.</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается, если не удается десериализовать сообщение.</exception>
    /// <exception cref="JsonException">Выбрасывается при ошибке десериализации JSON.</exception>
    public BaseMessage Deserialize(string messageJson)
    {
        try
        {
            var jObject = JObject.Parse(messageJson);
            var typeString = jObject["Type"]?.ToString() ?? string.Empty;

            if (Enum.TryParse<MessageType>(typeString, true, out var messageType))
            {
                var serializer = JsonSerializer.Create(_serializerSettings);

                switch (messageType)
                {
                    case MessageType.ChatMessage:
                        return jObject.ToObject<IncomingChatMessage>(serializer)
                               ?? throw new InvalidOperationException(
                                   "Не удалось десериализовать IncomingChatMessage.");
                    case MessageType.HistoryRequest:
                        return jObject.ToObject<HistoryRequest>(serializer)
                               ?? throw new InvalidOperationException("Не удалось десериализовать HistoryRequest.");
                    default:
                        _logger.LogWarning($"Неподдерживаемый тип сообщения: {typeString}");
                        return null;
                }
            }
            else
            {
                _logger.LogWarning($"Не удалось определить тип сообщения: {typeString}");
                return null!;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, $"Ошибка десериализации сообщения: {ex.Message}");
            return null!;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"Ошибка при десериализации: {ex.Message}");
            return null!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Неожиданная ошибка: {ex.Message}");
            return null!;
        }
    }
}