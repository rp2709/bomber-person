namespace BomberPerson.Core.State.NetworkMessages;

public class StopMessage : NetworkMessage
{
    public override MessageType Type => MessageType.Stop;
}
