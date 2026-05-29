using System;
using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

public class CountDownProgressMessage(DateTimeOffset dateTimeOffset) : FeedBackMessage
{
    protected override DateTimeOffset GetRealisationDate()
    {
        return dateTimeOffset;
    }

    public override void Process(State.State state)
    {
        if (state.CountDownValue > 0)
        {
            state.CountDownValue--;
        } 
        if (state.CountDownValue == 0)
        {
            state.CountDownValue = -1;
            state.CurrentPhase = State.State.Phase.Game;
        }
    }
}