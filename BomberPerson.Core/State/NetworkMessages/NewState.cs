using System.Collections.Generic;

namespace BomberPerson.Core.State.NetworkMessages;

public class NewStateMessage(State state) : NetworkMessage
{
    public override byte[] Serialize()
    {
        List<byte> buffer = new();
        buffer.AddRange(base.Serialize());
        
        // serialize state
        
        return buffer.ToArray();
    }
}