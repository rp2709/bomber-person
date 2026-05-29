using System.Numerics;
using BomberPerson.Core.Messages;
using BomberPerson.Core.Server;

namespace BomberPerson.Core.State.NetworkMessages;

public class StopMessage : NetworkMessage, ISimulationMessage
{
    public override MessageType Type => MessageType.Stop;
    public void Process(State state)
    {
        state.GetPlayer(SlotId).Velocity = Vector2.Zero;
    }
}
