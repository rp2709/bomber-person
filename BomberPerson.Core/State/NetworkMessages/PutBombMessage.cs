using System;
using System.IO;

namespace BomberPerson.Core.State.NetworkMessages;

public class PutBombMessage(uint x, uint y) : NetworkMessage
{
    public uint X { get; } = x > 0 ? x : throw new ArgumentException("X must be a positive integer.");
    public uint Y { get; } = y > 0 ? y : throw new ArgumentException("Y must be a positive integer.");

    public override MessageType Type => MessageType.PutBomb;

    public override byte[] Serialize()
    {
        MemoryStream memoryStream = new();
        BinaryWriter writer = new BinaryWriter(memoryStream);
        writer.Write(base.Serialize());
        writer.Write(X);
        writer.Write(Y);
        return memoryStream.ToArray();
    }
}
