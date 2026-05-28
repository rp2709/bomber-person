using System;
using System.Collections.Generic;
using System.Drawing;
using BomberPerson.Core.Messages;
using BomberPerson.Core.State.NetworkMessages;

namespace BomberPerson.Core.Server;

/// <summary>
/// The single authority over game state. It runs on one thread (the pipeline's
/// TransformManyBlock keeps MaxDegreeOfParallelism = 1), so it mutates the state without locks.
/// Every change emits a fresh <see cref="NewStateMessage"/> for the broadcast.
/// </summary>
public class Simulation(State.State state)
{
    private static readonly Color[] Palette =
    {
        Color.FromArgb(30,  60,  150),
        Color.FromArgb(200, 50,  50),
        Color.FromArgb(60,  180, 70),
        Color.FromArgb(220, 200, 40),
    };

    private static int countDownStart = 10;

    public IMessage[] ProcessMessage(IMessage message)
    {
        List<IMessage> messages = new();
        switch (message)
        {
            case NewPlayerMessage joined:
                if (Find(joined.Slot) == null)
                    state.Players.Add(new State.Player(joined.Slot, joined.Name, Palette[joined.Slot % Palette.Length]));
                break;

            case PlayerLeftMessage left:
                Remove(left.Slot);
                break;

            case FlipReadyMessage ready:
                Find(ready.SlotId)?.FlipReady();
                if (state.Players.TrueForAll(player => player.Ready))
                {
                    state.CountDownValue = countDownStart;
                    messages.Add(new CountDownProgressMessage(DateTimeOffset.Now + TimeSpan.FromSeconds(1)));
                }
                else
                {
                    state.CountDownValue = -1;
                }
                break;
            
            case CountDownProgressMessage progress:
                if (state.CountDownValue > 0)
                {
                    state.CountDownValue--;
                    messages.Add(new CountDownProgressMessage(DateTimeOffset.Now + TimeSpan.FromSeconds(1)));
                } else if (state.CountDownValue == 0)
                {
                    state.CountDownValue = -1;
                    state.CurrentPhase = State.State.Phase.Game;
                }
                break;
        }

        messages.Add(new NewStateMessage(state));
        
        return messages.ToArray();
    }

    private State.Player Find(int slot)
    {
        foreach (State.Player p in state.Players)
            if (p.Number == slot) return p;
        return null;
    }

    private void Remove(int slot)
    {
        for (int i = 0; i < state.Players.Count; i++)
            if (state.Players[i].Number == slot) { state.Players.RemoveAt(i); return; }
    }
}