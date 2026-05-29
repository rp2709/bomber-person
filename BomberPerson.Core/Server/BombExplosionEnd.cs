using System;
using System.Linq;
using BomberPerson.Core.Messages;

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
