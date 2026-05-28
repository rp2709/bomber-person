using System;
using System.Collections.Generic;

namespace BomberPerson.Core.Net;

/// <summary>
/// Base class for every server -> client message. The server encodes it with
/// <see cref="Serialize"/>; the <see cref="GameClient"/> decodes it with <see cref="Parse"/>.
/// Wire layout mirrors <see cref="Message"/>: [ MessageType : 1 byte ][ body ].
/// </summary>
public abstract class ServerMessage
{
    public abstract MessageType Type { get; }

    protected virtual void WriteBody(List<byte> body) { }

    public byte[] Serialize()
    {
        List<byte> bytes = new(4) { (byte)Type };
        WriteBody(bytes);
        return bytes.ToArray();
    }

    public static ServerMessage Parse(ReadOnlySpan<byte> payload)
    {
        if (payload.IsEmpty)
            throw new FormatException("Empty server message payload");

        MessageType type = (MessageType)payload[0];
        ReadOnlySpan<byte> body = payload[1..];
        return type switch
        {
            MessageType.Welcome when body.Length >= 1 => new Welcome(body[0]),
            MessageType.Snapshot => new Snapshot(body.ToArray()),
            MessageType.Rejected when body.Length >= 1 => new Rejected((RejectReason)body[0]),
            _ => throw new FormatException($"Unknown or malformed server message: {type}"),
        };
    }
}

/// <summary>Sent once after a successful connection: tells the client which player slot it owns.</summary>
public sealed class Welcome(int playerId) : ServerMessage
{
    public int PlayerId { get; } = playerId;
    public override MessageType Type => MessageType.Welcome;
    protected override void WriteBody(List<byte> body) => body.Add((byte)PlayerId);
}

/// <summary>A full game-state snapshot. The bytes are decoded by the state layer (next step).</summary>
public sealed class Snapshot(byte[] stateBytes) : ServerMessage
{
    public byte[] StateBytes { get; } = stateBytes;
    public override MessageType Type => MessageType.Snapshot;
    protected override void WriteBody(List<byte> body) => body.AddRange(StateBytes);
}

/// <summary>The server refused or dropped the connection.</summary>
public sealed class Rejected(RejectReason reason) : ServerMessage
{
    public RejectReason Reason { get; } = reason;
    public override MessageType Type => MessageType.Rejected;
    protected override void WriteBody(List<byte> body) => body.Add((byte)Reason);
}

public enum RejectReason : byte
{
    Unknown = 0,
    ServerFull = 1,
    GameAlreadyStarted = 2,
}