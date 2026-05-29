using System;
using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

public abstract class FeedBackMessage : IMessage, ISimulationMessage
{
    protected abstract DateTimeOffset GetRealisationDate();

    public static DateTimeOffset GetRealisationDate(IMessage msg)
    {
        return ((FeedBackMessage)msg).GetRealisationDate();
    }

    public abstract void Process(State.State state);
}