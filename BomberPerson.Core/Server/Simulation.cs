using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using BomberPerson.Core.Messages;
using BomberPerson.Core.State;
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

    private static readonly TimeSpan GameDuration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan CollisionSimulationStep = TimeSpan.FromMilliseconds(200);
    private const uint CellSize = 32;
    private const uint PlayerRadius = CellSize / 2u;
    private const uint BombRadius = PlayerRadius;
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
        if (state.CurrentDate == atTime) return;
        var deltaTime = atTime - state.CurrentDate;
        state.CurrentDate = atTime;

        if (state.CurrentPhase == State.State.Phase.Lobby)
        {
            return;
        }

        foreach (var player in state.Players)
        {
            player.Position += player.Velocity *  (float)deltaTime.TotalSeconds;
        }
    }

    private static FeedBackMessage NextEvent(IReadOnlyState state)
    {
        List<FeedBackMessage> comingEvents = [];
        if (state.CurrentPhase == State.State.Phase.Lobby && state.CountDownValue >= 0)
        {
            return new CountDownProgressMessage(DateTimeOffset.Now + TimeSpan.FromSeconds(1));
        }
        
        if (state.CurrentPhase == State.State.Phase.Game)
        {
            comingEvents.Add(new EndOfGameMessage(state.GameStartDate + GameDuration));
            comingEvents.AddRange(
                state.Bombs.Select(bomb => new BombExplosionStart(bomb.PositionX, bomb.PositionY, bomb.ExplosionDate))
                );

            comingEvents.AddRange(
                state.Explosions.Select(explosion => new BombExplosionEnd(explosion.EndDate))
                );
        }
        
        var first = comingEvents.MinBy(FeedBackMessage.GetRealisationDate);


        if (state.CurrentPhase != State.State.Phase.Game) return first;
        
        var playerEvent = _nextPlayerCollision(state, FeedBackMessage.GetRealisationDate(first));
        return playerEvent ?? first;
    }

    private static FeedBackMessage _nextPlayerCollision(IReadOnlyState state, DateTimeOffset until)
    {
        var interpolationState = state.Clone();
        var staticPlayers = new bool[state.Players.Count];
        var initiallyColliding = new bool[state.Players.Count];

        // Initial check for 'bad state'
        for (int i = 0; i < state.Players.Count; i++)
        {
            var player = state.Players[i];
            if (!player.IsAlive)
            {
                staticPlayers[i] = true;
                continue;
            }
            if (IsColliding(player.Position, state))
            {
                initiallyColliding[i] = true;
            }
        }

        for (DateTimeOffset step = state.CurrentDate; step < until; step += CollisionSimulationStep)
        {
            Interpolate(interpolationState, step);

            for (int i = 0; i < state.Players.Count; i++)
            {
                if (staticPlayers[i]) continue;
                var player = interpolationState.Players[i];
                
                if (player.Velocity == Vector2.Zero)
                {
                    staticPlayers[i] = true;
                }

                // check for death by explosion
                if (IsOverExplosion(player.Position, interpolationState))
                {
                    return new PlayerDeathMessage(player.Number, step);
                }

                // check for collision with blocking terrain elements (box, solid & border) or bomb
                bool colliding = IsColliding(player.Position, interpolationState);
                if (colliding)
                {
                    if (!initiallyColliding[i])
                    {
                        // stop player at previous step to avoid collision
                        return new PlayerCollisionMessage(player.Number, step - CollisionSimulationStep);
                    }
                }
                else
                {
                    // If not colliding anymore, clear the initiallyColliding flag
                    initiallyColliding[i] = false;
                }
            }

            if (staticPlayers.All(b => b)) break;
        }

        return null;
    }

    private static bool IsColliding(Vector2 position, IReadOnlyState state)
    {
        // Player is a point in this simulation (or we can use radius if needed, 
        // but PutBomb uses player.Position.X / 32, suggesting point-based cell check)
        uint x = (uint)(position.X / CellSize);
        uint y = (uint)(position.Y / CellSize);

        if (x >= state.Terrain.Width || y >= state.Terrain.Height) return true;

        var tile = state.Terrain[x, y];
        if (tile is Terrain.Type.Box or Terrain.Type.Solid or Terrain.Type.Border) return true;

        if (state.Bombs.Any(b => b.PositionX == x && b.PositionY == y)) return true;

        return false;
    }

    private static bool IsOverExplosion(Vector2 position, IReadOnlyState state)
    {
        uint x = (uint)(position.X / CellSize);
        uint y = (uint)(position.Y / CellSize);

        return state.Explosions.Any(e => e.PositionX == x && e.PositionY == y);
    }
}