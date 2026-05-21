using System.Drawing;
using System.Numerics;

namespace BomberPerson.Core.State;

public class Player(int number, Color color)
{
    public int Number { get; } = number;
    public Color Color { get; private set; } = color;

    private Vector2 Position { get; }= Vector2.Zero;
    private Vector2 Velocity { get; }= Vector2.Zero;
    public bool Ready { get; private set; } = false;
    public void FlipReady(){Ready = !Ready;}

    public void NextColor()
    {
        if (Ready)return;
        Color = Color.Aqua;
    }
}