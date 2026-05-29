using System.IO;
using BomberPerson.Core.Messages;
using BomberPerson.Core.Server;

namespace BomberPerson.Core.State.NetworkMessages;

public class JoinRequestMessage(string playerName) : NetworkMessage, ISimulationMessage
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

    public IMessage Process(State state)
    {
        // Handled by ClientHandler, but implementation here for completeness if needed in Simulation
        return null;
    }
}
