using Server.Models.Messages;

namespace Server.Utilities;

public interface IMessageSerializer
{
    string Serialize(BaseMessage message);
    BaseMessage Deserialize(string messageJson);
}