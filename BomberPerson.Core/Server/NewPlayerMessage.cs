using System.Drawing;
using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

/// <summary>Server-internal: a connection opened and was assigned <see cref="Slot"/>.</summary>
public class NewPlayerMessage(int slot, string name) : IMessage, ISimulationMessage
{
    public int Slot { get; } = slot;
    public string Name { get; } = name;

    public void Process(State.State state)
    {
        if (state.GetPlayer(Slot) == null)
            state.Players.Add(new State.Player(Slot, Name, Settings.Palette[Slot % Settings.Palette.Length]));
    }
}