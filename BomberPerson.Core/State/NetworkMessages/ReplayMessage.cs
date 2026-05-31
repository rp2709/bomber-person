using System.Numerics;
using BomberPerson.Core.Server;

namespace BomberPerson.Core.State.NetworkMessages;

/// <summary>
/// Client -> server: request a rematch. Resets the world to a fresh lobby with the same
/// connected players; everyone has to flip Ready again before the countdown restarts.
/// </summary>
public class ReplayMessage : NetworkMessage, ISimulationMessage
{
    public override MessageType Type => MessageType.Replay;

    public void Process(State state)
    {
        if (state.CurrentPhase != State.Phase.EndGame) return;

        state.CurrentPhase = State.Phase.Lobby;
        state.WinnerSlotId = -1;
        state.CountDownValue = -1;
        state.Bombs.Clear();
        state.Explosions.Clear();
        state.Terrain = Terrain.Generate();

        foreach (var player in state.Players)
        {
            player.IsAlive = true;
            player.Velocity = Vector2.Zero;
            player.Position = Vector2.Zero;
            if (player.Ready) player.FlipReady();
        }
    }
}
