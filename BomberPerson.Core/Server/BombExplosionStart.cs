using System;
using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

public class BombExplosionStart(uint positionX, uint positionY, DateTimeOffset realisationDate) : FeedBackMessage, ISimulationMessage
{
    private static readonly TimeSpan ExplosionDuration = TimeSpan.FromSeconds(1);

    public uint PositionX { get; } = positionX;
    public uint PositionY { get; } = positionY;

    protected override DateTimeOffset GetRealisationDate() => realisationDate;

    public override void Process(State.State state)
    {
        var bomb = state.Bombs.Find(b => b.PositionX == PositionX && b.PositionY == PositionY);
        if (bomb == null) return;

        state.Bombs.Remove(bomb);
        var endDate = DateTimeOffset.Now + ExplosionDuration;

        AddExplosion(state, PositionX, PositionY, endDate);

        var directions = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };
        foreach (var (dx, dy) in directions)
        {
            for (int i = 1; i <= bomb.BlastRadius; i++)
            {
                uint x = (uint)((int)PositionX + dx * i);
                uint y = (uint)((int)PositionY + dy * i);

                if (x >= state.Terrain.Width || y >= state.Terrain.Height)
                    break;

                var tile = state.Terrain[x, y];
                if (tile == State.Terrain.Type.Solid || tile == State.Terrain.Type.Border)
                    break;

                if (tile == State.Terrain.Type.Box)
                {
                    state.Terrain[x, y] = State.Terrain.Type.Empty;
                    AddExplosion(state, x, y, endDate);
                    break;
                }

                AddExplosion(state, x, y, endDate);
            }
        }

    }

    private static void AddExplosion(State.State state, uint x, uint y, DateTimeOffset endDate)
    {
        if (!state.Explosions.Exists(e => e.PositionX == x && e.PositionY == y))
        {
            state.Explosions.Add(new State.Explosion(x, y, endDate));
        }
    }
}
