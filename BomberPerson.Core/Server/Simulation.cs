using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
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
    private static readonly TimeSpan CollisionSimulationStep = TimeSpan.FromMilliseconds(50);
    public const uint CellSize = 32;
    public const uint PlayerRadius = 10;
    public const uint BombRadius = PlayerRadius;
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
        Vector2 playerCenter = position + new Vector2(PlayerRadius, PlayerRadius);
        uint centerCellX = (uint)playerCenter.X / CellSize;
        uint centerCellY = (uint)playerCenter.Y / CellSize;
        
        // check cells around the position
        for (uint i = centerCellX - 1; i <= centerCellX + 1; i++)
        {
            if(i >= state.Terrain.Width)continue;
            for (uint j = centerCellY - 1; j <= centerCellY + 1; j++)
            {
                if(j >= state.Terrain.Height)continue;
                if (state.Terrain[i,j] is Terrain.Type.Border or Terrain.Type.Solid or Terrain.Type.Box)
                {
                    Rectangle cell = new Rectangle(
                        (int)(i * CellSize),
                        (int)(j * CellSize),
                        (int)CellSize,
                        (int)CellSize);
                    
                    if(Intersects(cell,playerCenter,PlayerRadius))
                        return true;
                }
            }
        }
        
        
        // check for all bombs
        var squaredSummedRadius = (PlayerRadius + BombRadius) * (PlayerRadius + BombRadius);
        foreach (var bomb in state.Bombs)
        {
            Vector2 bombCenter = new Vector2(bomb.PositionX * CellSize + BombRadius, bomb.PositionY * CellSize + BombRadius);

            if ((bombCenter - playerCenter).LengthSquared() <= squaredSummedRadius)
                return true;
        }

        return false;
    }

    private static bool IsOverExplosion(Vector2 position, IReadOnlyState state)
    {
        Vector2 playerCenter = position + new Vector2(PlayerRadius, PlayerRadius);
        foreach (var explosion in state.Explosions)
        {
            Rectangle rect = new Rectangle(
                (int)(explosion.PositionX * CellSize),
                (int)(explosion.PositionY * CellSize),
                (int)CellSize,(int)CellSize);
            
            if(Intersects(rect,playerCenter,PlayerRadius))
                return true;
        }

        return false;
    }

    private static bool Intersects(Rectangle rectangle, Vector2 center, float radius)
    {
        float closestX = Math.Clamp(center.X, rectangle.Left, rectangle.Right);
        float closestY = Math.Clamp(center.Y, rectangle.Top, rectangle.Bottom);

        float dx = center.X - closestX;
        float dy = center.Y - closestY;

        return dx * dx + dy * dy <= radius * radius;
    }
}