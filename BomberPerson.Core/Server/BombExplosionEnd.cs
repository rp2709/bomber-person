using System;

namespace BomberPerson.Core.Server;

public class BombExplosionEnd(DateTimeOffset endDate) : FeedBackMessage
{
    protected override DateTimeOffset GetRealisationDate()
    {
        return endDate;
    }

    public override void Process(State.State state)
    {
        state.Explosions.RemoveAll(e => e.EndDate <= DateTimeOffset.Now);
    }
}
