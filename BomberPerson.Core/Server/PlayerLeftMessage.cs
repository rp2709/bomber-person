using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

/// <summary>Server-internal: the connection holding <see cref="Slot"/> disconnected.</summary>
public class PlayerLeftMessage(int slot) : IMessage
{
    public int Slot { get; } = slot;
}