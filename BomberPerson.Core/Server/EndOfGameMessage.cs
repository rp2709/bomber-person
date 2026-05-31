using System;
using System.Linq;

namespace BomberPerson.Core.Server;

public class EndOfGameMessage(DateTimeOffset realisationTime) : FeedBackMessage
{
    protected override DateTimeOffset GetRealisationDate()
    {
        return realisationTime;
    }

    public override void Process(State.State state)
    {
        if (state.CurrentPhase != State.State.Phase.Game) return;

        var alive = state.Players.Where(p => p.IsAlive).ToList();
        state.CurrentPhase = State.State.Phase.EndGame;
        state.WinnerSlotId = alive.Count == 1 ? alive[0].Number : -1;
    }
}