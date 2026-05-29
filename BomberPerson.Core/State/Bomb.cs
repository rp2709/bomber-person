using System;
using System.IO;

namespace BomberPerson.Core.State;

public class Bomb(DateTimeOffset explosionDate)
{
    public uint PositionX { get; set; }
    public uint PositionY { get; set; }

    public DateTimeOffset ExplosionDate { get; private set; } = explosionDate;

    public void Encode(BinaryWriter writer)
    {
        writer.Write(PositionX);
        writer.Write(PositionY);
        writer.Write(ExplosionDate.ToUnixTimeMilliseconds());
    }

    public static Bomb Decode(BinaryReader reader)
    {
        var x = reader.ReadUInt32();
        var y = reader.ReadUInt32();
        var date = DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64());
        return new Bomb(date)
        {
            PositionX = x,
            PositionY = y
        };
    }

    public Bomb Clone()
    {
        return new Bomb(ExplosionDate)
        {
            PositionX = PositionX,
            PositionY = PositionY
        };
    }
}