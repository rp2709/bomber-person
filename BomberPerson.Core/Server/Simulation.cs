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

    public IMessage[] ProcessMessage(IMessage message)
    {
        if (message is not ISimulationMessage simulationMessage)
            return [];
        
        Interpolate(state, DateTimeOffset.Now);
        
        simulationMessage.Process(state);
        
        var newStateMessage = new NewStateMessage(state.Clone());
        
        var result = NextEvent(state);
        if (result != null)
        {
            return [newStateMessage,result];    
        }
        
        return [newStateMessage];
    }

    public static void Interpolate(State.State state, DateTimeOffset atTime)
    {
        var deltaTime = atTime - state.Timestamp;
        state.Timestamp = atTime;

        if (state.CurrentPhase == State.State.Phase.Lobby)
        {
            return;
        }
    }

    public static FeedBackMessage NextEvent(State.IReadOnlyState state)
    {
        if (state.CurrentPhase == State.State.Phase.Lobby && state.CountDownValue >= 0)
        {
            return new CountDownProgressMessage(DateTimeOffset.Now + TimeSpan.FromSeconds(1));
        }

        return null;
    }
}