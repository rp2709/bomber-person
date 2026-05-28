using System.Collections.Generic;

namespace BomberPerson.Core.State.NetworkMessages;

public class NewStateMessage(State state) : NetworkMessage
{
    public override MessageType Type => MessageType.NewState;

    public override byte[] Serialize()
    {
        List<byte> buffer = new();
        buffer.AddRange(base.Serialize());

        buffer.AddRange(state.Encode());
        
        return buffer.ToArray();
    }
}