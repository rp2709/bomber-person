using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

/// <summary>Server-internal: the connection holding <see cref="Slot"/> disconnected.</summary>
public class PlayerLeftMessage(int slot) : IMessage, ISimulationMessage
{
    public int Slot { get; } = slot;

    public void Process(State.State state)
    {
        for (int i = 0; i < state.Players.Count; i++)
        {
            if (state.Players[i].Number == Slot)
            {
                state.Players.RemoveAt(i);
                break;
            }
        }
    }
}