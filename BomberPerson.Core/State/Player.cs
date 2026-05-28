using System.Drawing;
using System.IO;
using System.Numerics;

namespace BomberPerson.Core.State;

public class Player(int number, Color color)
{
    public int Number { get; private set; } = number;
    public Color Color { get; private set; } = color;

    public Vector2 Position { get; set; }= Vector2.Zero;
    public Vector2 Velocity { get; set; }= Vector2.Zero;
    public bool Ready { get; private set; } = false;
    public void FlipReady(){Ready = !Ready;}

    public void NextColor()
    {
        if (Ready)return;
        Color = Color.Aqua;
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Number);
        writer.Write(Color.ToArgb());
        writer.Write(Position.X);
        writer.Write(Position.Y);
        writer.Write(Velocity.X);
        writer.Write(Velocity.Y);
        writer.Write(Ready);
    }

    public static Player Decode(BinaryReader reader)
    {
        var number = reader.ReadInt32();
        var color = Color.FromArgb(reader.ReadInt32());
        var player = new Player(number, color)
        {
            Position = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
            Velocity = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
            Ready = reader.ReadBoolean()
        };
        return player;
    }
}