using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Server.Models.Messages;
using Microsoft.Extensions.Logging;

namespace Server.Utilities;

public class MessageSerializer : IMessageSerializer
{
    private readonly ILogger<MessageSerializer> _logger;
    private readonly JsonSerializerSettings _serializerSettings;

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

    public string Serialize(BaseMessage message)
    {
        return JsonConvert.SerializeObject(message, _serializerSettings);
    }

    public BaseMessage Deserialize(string messageJson)
    {
        try
        {
            var jObject = JObject.Parse(messageJson);
            var typeString = jObject["Type"]?.ToString() ?? string.Empty;

            if (Enum.TryParse<MessageType>(typeString, true, out var messageType))
            {
                var serializer = JsonSerializer.Create(_serializerSettings);

                return messageType switch
                {
                    MessageType.ChatMessage => jObject.ToObject<IncomingChatMessage>(serializer)
                                               ?? throw new InvalidOperationException(
                                                   "Не удалось десериализовать IncomingChatMessage."),
                    MessageType.HistoryRequest => jObject.ToObject<HistoryRequest>(serializer)
                                                  ?? throw new InvalidOperationException(
                                                      "Не удалось десериализовать HistoryRequest."),
                    _ => throw new NotSupportedException($"Неподдерживаемый тип сообщения: {typeString}.")
                };
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