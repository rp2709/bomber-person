using System;

namespace BomberPerson.Core.State.NetworkMessages;

/// <summary>
/// Server -> client: a full snapshot of the game state. The payload is encoded once at
/// construction (on the single simulation thread) so that the per-client writers can never
/// read the live state while it is being mutated.
/// </summary>
public class NewStateMessage : NetworkMessage
{
    public State State { get; }
    private readonly byte[] encoded;

    public NewStateMessage(State state)
    {
        State = state;
        encoded = state.Encode();
    }

    public override MessageType Type => MessageType.NewState;

    public override byte[] Serialize()
    {
        byte[] buffer = new byte[1 + encoded.Length];
        buffer[0] = (byte)Type;
        Array.Copy(encoded, 0, buffer, 1, encoded.Length);
        return buffer;
    }
}