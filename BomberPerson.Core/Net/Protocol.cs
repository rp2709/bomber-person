using System;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BomberPerson.Core.Net;

/// <summary>
/// Length-prefixed framing over a byte stream. TCP is a stream, not a sequence of
/// messages, so every message is delimited by a length header.
/// Frame layout: [ payload length : uint16 big-endian ][ payload : N bytes ].
/// The payload's first byte is the <see cref="MessageType"/>; the rest is the body.
/// </summary>
public static class Protocol
{
    public const int MaxPayloadSize = ushort.MaxValue; // 65535 bytes, plenty for a 16x16 grid

    public static async Task WriteFrameAsync(Stream stream, ReadOnlyMemory<byte> payload,
        CancellationToken ct = default)
    {
        if (payload.Length > MaxPayloadSize)
            throw new ArgumentOutOfRangeException(nameof(payload),
                $"Frame too large: {payload.Length} > {MaxPayloadSize}");

        // Header and payload go out as a single write so they land in the same TCP segment.
        byte[] buffer = new byte[2 + payload.Length];
        BinaryPrimitives.WriteUInt16BigEndian(buffer, (ushort)payload.Length);
        payload.Span.CopyTo(buffer.AsSpan(2));

        await stream.WriteAsync(buffer, ct);
        await stream.FlushAsync(ct);
    }

    /// <summary>
    /// Reads exactly one frame. Returns the payload (type byte + body), or <c>null</c>
    /// when the peer closed the connection cleanly at a frame boundary.
    /// Throws if the stream ends in the middle of a frame.
    /// </summary>
    public static async Task<byte[]?> ReadFrameAsync(Stream stream, CancellationToken ct = default)
    {
        byte[] header = new byte[2];
        int read = await stream.ReadAsync(header.AsMemory(0, 2), ct);
        if (read == 0) return null;                 // clean disconnect at a frame boundary
        if (read < 2)
            await stream.ReadExactlyAsync(header.AsMemory(read, 2 - read), ct);

        int length = BinaryPrimitives.ReadUInt16BigEndian(header);
        byte[] payload = new byte[length];
        if (length > 0)
            await stream.ReadExactlyAsync(payload.AsMemory(0, length), ct);
        return payload;
    }
}
