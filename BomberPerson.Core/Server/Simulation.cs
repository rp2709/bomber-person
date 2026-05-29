using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        foreach (var player in state.Players)
        {
            player.Position += player.Velocity *  (float)deltaTime.TotalSeconds;
        }
    }

    public static FeedBackMessage NextEvent(State.State state)
    {
        if (state.CurrentPhase == State.State.Phase.Game)
        {
            foreach (var player in state.Players)
            {
                if (!player.IsAlive) continue;

                int tileX = (int)(player.Position.X / 32);
                int tileY = (int)(player.Position.Y / 32);

                foreach (var explosion in state.Explosions)
                {
                    if (tileX == explosion.PositionX && tileY == explosion.PositionY)
                    {
                        player.IsAlive = false;
                        break;
                    }
                }
            }
        }

        FeedBackMessage bestEvent = null;
        DateTimeOffset bestDate = DateTimeOffset.MaxValue;

        if (state.CurrentPhase == State.State.Phase.Game)
        {
            foreach (var bomb in state.Bombs)
            {
                if (bomb.ExplosionDate < bestDate)
                {
                    bestDate = bomb.ExplosionDate;
                    bestEvent = new BombExplosionStart(bomb.PositionX, bomb.PositionY, bomb.ExplosionDate);
                }
            }

            foreach (var explosion in state.Explosions)
            {
                if (explosion.EndDate < bestDate)
                {
                    bestDate = explosion.EndDate;
                    bestEvent = new BombExplosionEnd(explosion.EndDate);
                }
            }
        }

        if (state.CurrentPhase == State.State.Phase.Lobby && state.CountDownValue >= 0)
        {
            var countdownDate = DateTimeOffset.Now + TimeSpan.FromSeconds(1);
            if (countdownDate < bestDate)
            {
                bestEvent = new CountDownProgressMessage(countdownDate);
            }
        }

        return bestEvent;
    }
}