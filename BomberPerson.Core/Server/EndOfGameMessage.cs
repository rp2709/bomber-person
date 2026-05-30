using System;

namespace BomberPerson.Core.Server;

public class EndOfGameMessage(DateTimeOffset realisationTime) : FeedBackMessage
{
    protected override DateTimeOffset GetRealisationDate()
    {
        return realisationTime;
    }

    public override void Process(State.State state)
    {
        throw new NotImplementedException();
    }
}