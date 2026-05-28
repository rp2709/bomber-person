using System;
using System.IO;

namespace BomberPerson.Core.State;

public class Bomb(DateTime explosionDate)
{
    public uint PositionX { get; set; }
    public uint PositionY { get; set; }

    public DateTime ExplosionDate { get; private set; } = explosionDate;

    public void Encode(BinaryWriter writer)
    {
        writer.Write(PositionX);
        writer.Write(PositionY);
        writer.Write(ExplosionDate.ToBinary());
    }

    public static Bomb Decode(BinaryReader reader)
    {
        var x = reader.ReadUInt32();
        var y = reader.ReadUInt32();
        var date = DateTime.FromBinary(reader.ReadInt64());
        return new Bomb(date)
        {
            PositionX = x,
            PositionY = y
        };
    }
}