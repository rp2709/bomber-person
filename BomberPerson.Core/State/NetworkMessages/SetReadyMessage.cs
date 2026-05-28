namespace BomberPerson.Core.State.NetworkMessages;

/// <summary>
/// Client -> server: the local player toggles its ready state. The server knows which slot the
/// message belongs to from the connection, so no id travels on the wire.
/// </summary>
public class SetReadyMessage(bool ready) : NetworkMessage
{
    public bool Ready { get; } = ready;
    public override MessageType Type => MessageType.SetReady;
    public override byte[] Serialize() => [(byte)Type, (byte)(Ready ? 1 : 0)];
}