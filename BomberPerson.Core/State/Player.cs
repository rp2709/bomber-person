using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;

namespace BomberPerson.Core.State;

public interface IReadOnlyPlayer
{
    int Number { get; }
    string Name { get; }
    Color Color { get; }
    Vector2 Position { get; }
    Vector2 Velocity { get; }
    bool Ready { get; }
}

public class Player(int number, string name, Color color) : IReadOnlyPlayer
{
    public int Number { get; private set; } = number;
    public string Name { get; private set; } = name;
    public Color Color { get; private set; } = color;

    public Vector2 Position { get; set; }= Vector2.Zero;
    public Vector2 Velocity { get; set; }= Vector2.Zero;
    public bool Ready { get; private set; } = false;
    public void FlipReady(){Ready = !Ready;}
    public void SetReady(bool ready){Ready = ready;}
    public void Encode(BinaryWriter writer)
    {
        writer.Write(Number);
        writer.Write(Name);
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
        var name = reader.ReadString();
        var color = Color.FromArgb(reader.ReadInt32());
        var player = new Player(number, name, color)
        {
            Position = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
            Velocity = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
            Ready = reader.ReadBoolean()
        };
        return player;
    }

    public Player Clone()
    {
        return new Player(Number, Name, Color)
        {
            Position = Position,
            Velocity = Velocity,
            Ready = Ready
        };
    }
}