using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

/// <summary>Server-internal: a connection opened and was assigned <see cref="Slot"/>.</summary>
public class NewPlayerMessage(int slot) : IMessage
{
    public int Slot { get; } = slot;
}