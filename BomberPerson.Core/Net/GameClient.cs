using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace BomberPerson.Core.Net;

/// <summary>
/// Client-side network endpoint, used identically by the host (over loopback) and by remote
/// players. The MonoGame loop talks only to this class, never to the server directly.
///
/// Outgoing: an <see cref="ActionBlock{T}"/> with MaxDegreeOfParallelism = 1 serializes
/// access to the socket, so the game loop can fire messages from any thread without locks.
/// Incoming: a background read loop parses frames and posts <see cref="ServerMessage"/>s into
/// a <see cref="BufferBlock{T}"/> that the game loop drains once per frame.
/// </summary>
public sealed class GameClient : IDisposable
{
    private TcpClient? tcp;
    private ActionBlock<Message>? outbound;
    private readonly BufferBlock<ServerMessage> inbound = new();
    private CancellationTokenSource? cts;
    private Task? readLoop;
    private int disconnected;

    public bool IsConnected => Volatile.Read(ref disconnected) == 0 && tcp?.Connected == true;

    /// <summary>Source of incoming server messages. Link a block to it, or call <see cref="TryDrain"/>.</summary>
    public ISourceBlock<ServerMessage> Incoming => inbound;

    /// <summary>
    /// Connects with a few retries to absorb the host start-up race (the server may not be
    /// listening yet when the host's own client connects) and slow remote joins.
    /// </summary>
    public async Task ConnectAsync(string host, int port, int retries = 10, int retryDelayMs = 100, CancellationToken ct = default)
    {
        TcpClient tcpClient = new TcpClient { NoDelay = true };
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                await tcpClient.ConnectAsync(host, port, ct);
                break;
            }
            catch (SocketException) when (attempt < retries)
            {
                await Task.Delay(retryDelayMs, ct);
            }
            catch
            {
                tcpClient.Dispose();
                throw;
            }
        }

        tcp = tcpClient;
        cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        NetworkStream stream = tcpClient.GetStream();
        CancellationToken token = cts.Token;

        outbound = new ActionBlock<Message>(
            async msg =>
            {
                try { await Protocol.WriteFrameAsync(stream, msg.Serialize(), token); }
                catch { OnConnectionLost(); }
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, CancellationToken = token });

        readLoop = Task.Run(() => ReadLoopAsync(stream, token), CancellationToken.None);
    }

    /// <summary>Queues a message to be sent. Safe to call from the game thread.</summary>
    public void Send(Message message) => outbound?.Post(message);

    /// <summary>Pulls every pending server message. Call once per game-loop update.</summary>
    public IList<ServerMessage> TryDrain()
        => inbound.TryReceiveAll(out IList<ServerMessage>? items) && items is not null
            ? items
            : Array.Empty<ServerMessage>();

    /// <summary>Graceful leave: flushes a <see cref="LeaveMessage"/> before closing the socket.</summary>
    public async Task LeaveAsync()
    {
        if (outbound is not null && IsConnected)
        {
            outbound.Post(new LeaveMessage());
            outbound.Complete();
            try { await outbound.Completion; } catch { /* ignore flush errors on the way out */ }
        }
        OnConnectionLost();
    }

    private async Task ReadLoopAsync(NetworkStream stream, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                byte[]? payload = await Protocol.ReadFrameAsync(stream, ct);
                if (payload is null) break;             // clean disconnect
                inbound.Post(ServerMessage.Parse(payload));
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { /* socket reset or malformed frame -> treat as disconnect */ }
        finally { OnConnectionLost(); }
    }

    private void OnConnectionLost()
    {
        if (Interlocked.Exchange(ref disconnected, 1) != 0) return;
        inbound.Complete();
        outbound?.Complete();
        try { cts?.Cancel(); } catch { /* already disposed */ }
        try { tcp?.Close(); } catch { /* already closed */ }
    }

    public void Dispose()
    {
        OnConnectionLost();
        cts?.Dispose();
        tcp?.Dispose();
    }
}