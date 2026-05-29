using System;
using System.IO;

namespace BomberPerson.Core.State;

public class Explosion(uint positionX, uint positionY, DateTimeOffset endDate)
{
    public uint PositionX { get; set; } = positionX;
    public uint PositionY { get; set; } = positionY;
    public DateTimeOffset EndDate { get; private set; } = endDate;

    public void Encode(BinaryWriter writer)
    {
        writer.Write(PositionX);
        writer.Write(PositionY);
        writer.Write(EndDate.ToUnixTimeMilliseconds());
    }

    public static Explosion Decode(BinaryReader reader)
    {
        var x = reader.ReadUInt32();
        var y = reader.ReadUInt32();
        var date = DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64());
        return new Explosion(x, y, date);
    }

    public Explosion Clone()
    {
        return new Explosion(PositionX, PositionY, EndDate);
    }
}
