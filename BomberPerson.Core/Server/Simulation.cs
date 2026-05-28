using System;
using System.Collections.Generic;
using BomberPerson.Core.Net;

namespace BomberPerson.Core.Server;

/// <summary>
/// The single authority over game state. It runs on one thread (the pipeline's
/// <c>TransformManyBlock</c> keeps MaxDegreeOfParallelism = 1), so it mutates the state
/// without any locks. Inputs and lifecycle events mutate the state; a <see cref="Tick"/>
/// advances the world and emits one encoded snapshot to broadcast.
/// </summary>
public sealed class Simulation(State.State state)
{
    public IEnumerable<byte[]> Process(ServerCommand command)
    {
        switch (command)
        {
            case PlayerJoined joined:
                state.AddPlayer(joined.PlayerId);
                break;
            case PlayerLeft left:
                state.RemovePlayer(left.PlayerId);
                break;
            case PlayerInput input:
                ApplyInput(input.PlayerId, input.Message);
                break;
            case Tick tick:
                Advance(tick.DeltaSeconds);
                return new[] { new Snapshot(state.Encode()).Serialize() };
        }
        return Array.Empty<byte[]>();
    }

    private void ApplyInput(int playerId, Message message)
    {
        State.Player? player = state.FindPlayer(playerId);
        if (player is null) return;

        switch (message)
        {
            case ToggleReadyMessage:
                player.FlipReady();
                break;
            case CycleColorMessage:
                player.NextColor();
                break;
            case MoveMessage:
            case PlaceBombMessage:
                // Buffered intent; resolved in Advance once the gameplay model exists.
                break;
        }
    }

    private void Advance(float deltaSeconds)
    {
        // Movement, bomb timers and explosions land here with the gameplay model.
    }
}
