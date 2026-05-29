using System;
using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

public class CountDownProgressMessage(DateTimeOffset dateTimeOffset) : FeedBackMessage
{
    protected override DateTimeOffset GetRealisationDate()
    {
        return dateTimeOffset;
    }

    public override IMessage Process(State.State state)
    {
        IMessage msg = null;
        if (state.CountDownValue > 0)
        {
            state.CountDownValue--;
            msg = new CountDownProgressMessage(DateTimeOffset.Now + TimeSpan.FromSeconds(1));
        } 
        if (state.CountDownValue == 0)
        {
            state.CountDownValue = -1;
            state.CurrentPhase = State.State.Phase.Game;
        }
        return msg;
    }
}