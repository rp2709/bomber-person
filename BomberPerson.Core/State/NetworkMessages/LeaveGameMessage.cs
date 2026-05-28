namespace BomberPerson.Core.State.NetworkMessages;

public class LeaveGameMessage : NetworkMessage
{
    public override MessageType Type => MessageType.LeaveGame;
}