using System;
using System.Linq;

namespace BomberPerson.Core.Server;

public class PlayerDeathMessage(int playerSlot, DateTimeOffset realisationDate) : FeedBackMessage
{
    public int PlayerSlot { get; } = playerSlot;

    protected override DateTimeOffset GetRealisationDate() => realisationDate;

    public override void Process(State.State state)
    {
        var player = state.GetPlayer(PlayerSlot);
        if (player != null)
        {
            player.IsAlive = false;
            player.Velocity = System.Numerics.Vector2.Zero;
        }

        if (state.CurrentPhase != State.State.Phase.Game) return;

        var alive = state.Players.Where(p => p.IsAlive).ToList();
        if (alive.Count <= 1)
        {
            state.CurrentPhase = State.State.Phase.EndGame;
            state.WinnerSlotId = alive.Count == 1 ? alive[0].Number : -1;
        }
    }
}
