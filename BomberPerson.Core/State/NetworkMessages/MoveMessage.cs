using System.Collections.Generic;
using System.IO;
using System.Numerics;
using BomberPerson.Core.Messages;
using BomberPerson.Core.Server;

namespace BomberPerson.Core.State.NetworkMessages;

public class MoveMessage(MoveMessage.MoveDirection direction) : NetworkMessage, ISimulationMessage
{
    private readonly Vector2[] directionVectors = [
        new Vector2(0,1),
        new Vector2(0,-1),
        new Vector2(-1,0),
        new Vector2(1,0)
    ];
    public enum MoveDirection
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

    public void Process(State state)
    {
        const float speed = 1.5f;
        state.GetPlayer(SlotId).Velocity = directionVectors[(int)Direction] * speed;
    }
}