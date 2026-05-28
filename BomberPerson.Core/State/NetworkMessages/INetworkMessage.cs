using BomberPerson.Core.Messages;

namespace BomberPerson.Core.State.NetworkMessages;

public abstract class NetworkMessage : IMessage
{
    public virtual byte[] Serialize() {return this.GetType().GUID.ToByteArray();}
}