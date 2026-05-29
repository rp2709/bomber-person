using System;
using BomberPerson.Core.Messages;
using BomberPerson.Core.Server;

namespace BomberPerson.Core.State.NetworkMessages;

/// <summary>
/// Client -> server: the local player toggles its ready state. The server knows which slot the
/// message belongs to from the connection, so no id travels on the wire.
/// </summary>
public class FlipReadyMessage : NetworkMessage, ISimulationMessage
{
    private static int countDownStart = 10;
    public override MessageType Type { get; } = MessageType.FlipReady;
    public IMessage Process(State state)
    {
        state.GetPlayer(SlotId)?.FlipReady();
        if (state.Players.TrueForAll(player => player.Ready))
        {
            state.CountDownValue = countDownStart;
            return new CountDownProgressMessage(DateTimeOffset.Now + TimeSpan.FromSeconds(1));
        }

        state.CountDownValue = -1;
        return null;
    }
}