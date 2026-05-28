using System;
using System.Collections.Generic;
using BomberPerson.Core.State;

namespace BomberPerson.Core.Net;

/// <summary>
/// Base class for every client -> server message. This is the type that flows through the
/// server's dataflow pipeline. Wire layout: [ MessageType : 1 byte ][ body ]; most
/// messages carry no body.
/// </summary>
public abstract class Message
{
    public abstract MessageType Type { get; }

    protected virtual void WriteBody(List<byte> body) { }

    public byte[] Serialize()
    {
        List<byte> bytes = new List<byte>(4) { (byte)Type };
        WriteBody(bytes);
        return bytes.ToArray();
    }

    public static Message Deserialize(ReadOnlySpan<byte> payload)
    {
        if (payload.IsEmpty)
            throw new FormatException("Empty message payload");

        MessageType type = (MessageType)payload[0];
        ReadOnlySpan<byte> body = payload[1..];
        return type switch
        {
            MessageType.Move when body.Length >= 1 => new MoveMessage((Direction)body[0]),
            MessageType.PlaceBomb => new PlaceBombMessage(),
            MessageType.ToggleReady => new ToggleReadyMessage(),
            MessageType.CycleColor => new CycleColorMessage(),
            MessageType.Leave => new LeaveMessage(),
            _ => throw new FormatException($"Unknown or malformed client message: {type}"),
        };
    }
}

public sealed class MoveMessage(Direction direction) : Message
{
    public Direction Direction { get; } = direction;
    public override MessageType Type => MessageType.Move;
    protected override void WriteBody(List<byte> body) => body.Add((byte)Direction);
}

public sealed class PlaceBombMessage : Message
{
    public override MessageType Type => MessageType.PlaceBomb;
}

public sealed class ToggleReadyMessage : Message
{
    public override MessageType Type => MessageType.ToggleReady;
}

public sealed class CycleColorMessage : Message
{
    public override MessageType Type => MessageType.CycleColor;
}

public sealed class LeaveMessage : Message
{
    public override MessageType Type => MessageType.Leave;
}