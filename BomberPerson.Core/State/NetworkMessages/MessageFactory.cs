using System.IO;
using BomberPerson.Core.Messages;

namespace BomberPerson.Core.State.NetworkMessages;

public static class NetworkMessageFactory
{
    public static IMessage FromStream(Stream stream)
    {
        int typeByte = stream.ReadByte();
        if (typeByte == -1) return null;

        MessageType type = (MessageType)typeByte;

        return type switch
        {
            MessageType.LobbyFull => new LobbyFull(),
            MessageType.NewState => new NewStateMessage(null), // TODO: Deserialize state
            MessageType.LeaveGame => new LeaveGameMessage(),
            _ => throw new InvalidDataException($"Unknown message type: {typeByte}")
        };
    }
}