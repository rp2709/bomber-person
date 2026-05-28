namespace BomberPerson.Core.State.NetworkMessages;

/// <summary>
/// Client -> server: the local player toggles its ready state. The server knows which slot the
/// message belongs to from the connection, so no id travels on the wire.
/// </summary>
public class FlipReadyMessage : NetworkMessage
{
    public override MessageType Type { get; } = MessageType.FlipReady;
}