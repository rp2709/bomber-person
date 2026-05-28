using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

/// <summary>
/// Server-internal: the player at <see cref="Slot"/> changed its ready state. Built by the
/// ClientHandler from a wire SetReadyMessage, tagged with the connection's slot.
/// </summary>
public class PlayerReadyMessage(int slot, bool ready) : IMessage
{
    public int Slot { get; } = slot;
    public bool Ready { get; } = ready;
}