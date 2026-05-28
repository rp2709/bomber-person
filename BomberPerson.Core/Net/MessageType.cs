namespace BomberPerson.Core.Net;

/// <summary>
/// Identifies a message on the wire. The first byte of every frame payload.
/// Client -> server values stay in 1..99, server -> client in 100+.
/// </summary>
public enum MessageType : byte
{
    // client -> server
    Move = 1,
    PlaceBomb = 2,
    ToggleReady = 3,
    CycleColor = 4,
    Leave = 5,

    // server -> client
    Welcome = 100,
    Snapshot = 101,
    Rejected = 102,
}
