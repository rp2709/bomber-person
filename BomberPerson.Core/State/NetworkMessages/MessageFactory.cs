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
            MessageType.NewState => new NewStateMessage(State.Decode(stream)),
            MessageType.LeaveGame => new LeaveGameMessage(),
            MessageType.SetReady => new SetReadyMessage(stream.ReadByte() == 1),
            _ => throw new InvalidDataException($"Unknown message type: {typeByte}")
        };
    }
}