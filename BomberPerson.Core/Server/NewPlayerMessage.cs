using System.Drawing;
using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

/// <summary>Server-internal: a connection opened and was assigned <see cref="Slot"/>.</summary>
public class NewPlayerMessage(int slot, string name) : IMessage, ISimulationMessage
{
    private static readonly Color[] Palette =
    {
        Color.FromArgb(30,  60,  150),
        Color.FromArgb(200, 50,  50),
        Color.FromArgb(60,  180, 70),
        Color.FromArgb(220, 200, 40),
    };

    public int Slot { get; } = slot;
    public string Name { get; } = name;

    public IMessage Process(State.State state)
    {
        if (state.GetPlayer(Slot) == null)
            state.Players.Add(new State.Player(Slot, Name, Palette[Slot % Palette.Length]));
        return null;
    }
}