using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BomberPerson.Core.Messages;

namespace BomberPerson.Core.State.NetworkMessages;

public static class NetworkMessageFactory
{
    public static async Task<IMessage> FromStreamAsync(Stream stream, CancellationToken token, int slotId = 0)
    {
        byte[] buffer = new byte[1];
        int read = await stream.ReadAsync(buffer, token);
        if (read == 0) return null;
        int typeByte = buffer[0];

        MessageType type = (MessageType)typeByte;

        NetworkMessage message = type switch
        {
            MessageType.LobbyFull => new LobbyFull(),
            MessageType.NewState => await Task.Run(() => new NewStateMessage(State.Decode(stream)), token),
            MessageType.LeaveGame => new LeaveGameMessage(),
            MessageType.Move => new MoveMessage((MoveMessage.MoveDirection)stream.ReadByte()),
            MessageType.Stop => new StopMessage(),
            MessageType.PutBomb => new PutBombMessage(new BinaryReader(stream).ReadUInt32(), new BinaryReader(stream).ReadUInt32()),
            MessageType.FlipReady => new FlipReadyMessage(),
            MessageType.JoinRequest => new JoinRequestMessage(new BinaryReader(stream).ReadString()),
            _ => throw new InvalidDataException($"Unknown message type: {typeByte}")
        };

        message.SlotId = slotId;
        return message;
    }

    public static IMessage FromStream(Stream stream, int slotId = 0)
    {
        int typeByte = stream.ReadByte();
        if (typeByte == -1) return null;

        MessageType type = (MessageType)typeByte;

        NetworkMessage message = type switch
        {
            MessageType.LobbyFull => new LobbyFull(),
            MessageType.NewState => new NewStateMessage(State.Decode(stream)),
            MessageType.LeaveGame => new LeaveGameMessage(),
            MessageType.Move => new MoveMessage((MoveMessage.MoveDirection)stream.ReadByte()),
            MessageType.Stop => new StopMessage(),
            MessageType.PutBomb => new PutBombMessage(new BinaryReader(stream).ReadUInt32(), new BinaryReader(stream).ReadUInt32()),
            MessageType.FlipReady => new FlipReadyMessage(),
            MessageType.JoinRequest => new JoinRequestMessage(new BinaryReader(stream).ReadString()),
            _ => throw new InvalidDataException($"Unknown message type: {typeByte}")
        };

        message.SlotId = slotId;
        return message;
    }
}