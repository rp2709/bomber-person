namespace BomberPerson.Core.State.NetworkMessages;

public class LobbyFull : NetworkMessage
{
    public override MessageType Type => MessageType.LobbyFull;
}