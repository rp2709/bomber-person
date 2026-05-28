using System;
using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

public abstract class FeedBackMessage : IMessage
{
    protected abstract DateTimeOffset GetRealisationDate();

    public static DateTimeOffset GetRealisationDate(IMessage msg)
    {
        return ((FeedBackMessage)msg).GetRealisationDate();
    }
}