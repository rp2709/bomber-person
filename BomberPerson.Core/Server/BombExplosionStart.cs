using System;
using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

public class BombExplosionStart : FeedBackMessage, ISimulationMessage
{
    protected override DateTimeOffset GetRealisationDate()
    {
        throw new NotImplementedException();
    }

    public override void Process(State.State state)
    {
        throw new NotImplementedException();
    }
}