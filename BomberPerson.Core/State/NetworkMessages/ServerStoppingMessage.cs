namespace BomberPerson.Core.State.NetworkMessages;

public class ServerStoppingMessage : NetworkMessage
{
    public override MessageType Type => MessageType.ServerStopping;
}
