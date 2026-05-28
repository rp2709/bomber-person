using BomberPerson.Core.Messages;

namespace BomberPerson.Core.State.NetworkMessages;

public abstract class NetworkMessage : IMessage
{
    public int SlotId;
    public abstract MessageType Type { get; }
    public virtual byte[] Serialize() { return [(byte)Type]; }
}