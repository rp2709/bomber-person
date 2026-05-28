using System;

namespace BomberPerson.Core.Server;

public class CountDownProgressMessage(DateTimeOffset dateTimeOffset) : FeedBackMessage
{
    protected override DateTimeOffset GetRealisationDate()
    {
        return dateTimeOffset;
    }
}