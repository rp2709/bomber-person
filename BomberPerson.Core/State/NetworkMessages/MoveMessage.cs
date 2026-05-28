using System.Collections.Generic;
using System.IO;

namespace BomberPerson.Core.State.NetworkMessages;

public class MoveMessage(MoveMessage.MoveDirection direction) : NetworkMessage
{
    public enum MoveDirection : byte
    {
        Up,
        Down,
        Left,
        Right
    }
    
    public MoveDirection Direction { get;} = direction;
    public override MessageType Type => MessageType.Move;

    public override byte[] Serialize()
    { 
       MemoryStream memoryStream = new();
       BinaryWriter writer = new BinaryWriter(memoryStream);
       writer.Write(base.Serialize());
       writer.Write((byte)Direction);
       return memoryStream.ToArray(); 
    }
}