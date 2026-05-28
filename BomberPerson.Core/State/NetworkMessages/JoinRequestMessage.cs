using System.IO;

namespace BomberPerson.Core.State.NetworkMessages;

public class JoinRequestMessage(string playerName) : NetworkMessage
{
    public string PlayerName { get; } = playerName;

    public override MessageType Type => MessageType.JoinRequest;

    public override byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write((byte)Type);
        writer.Write(PlayerName);
        return ms.ToArray();
    }
}
