using BomberPerson.Core.Messages;
using BomberPerson.Core.Server;

namespace BomberPerson.Core.State.NetworkMessages;

public class LeaveGameMessage : NetworkMessage, ISimulationMessage
{
    public override MessageType Type => MessageType.LeaveGame;

    public IMessage Process(State state)
    {
        for (int i = 0; i < state.Players.Count; i++)
        {
            if (state.Players[i].Number == SlotId)
            {
                state.Players.RemoveAt(i);
                break;
            }
        }
        return null;
    }
}