using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BomberPerson.Core.Messages;
using BomberPerson.Core.State.NetworkMessages;

namespace BomberPerson.Core.Network;

/// <summary>
/// Client-side TCP endpoint. The host (connecting to 127.0.0.1) and remote players use it
/// identically. Outbound messages go through an <see cref="ActionBlock{T}"/> with
/// MaxDegreeOfParallelism = 1, so the socket has a single writer and needs no locks.
/// </summary>
public class Client : IDisposable
{
    private TcpClient tcp;
    private ActionBlock<IMessage> outbound;
    private CancellationTokenSource cts;
    private int disconnected;

    public bool IsConnected => Volatile.Read(ref disconnected) == 0 && tcp != null && tcp.Connected;

    /// <summary>
    /// Connects with a few retries so the host's own client can wait for its server's listener
    /// to come up, and slow remote joins succeed too. Returns false if the connection fails.
    /// </summary>
    public async Task<bool> ConnectAsync(string host, int port, int retries = 10, int retryDelayMs = 100)
    {
        TcpClient tcpClient = new TcpClient { NoDelay = true };
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                await tcpClient.ConnectAsync(host, port);
                break;
            }
            catch (SocketException) when (attempt < retries)
            {
                await Task.Delay(retryDelayMs);
            }
            catch
            {
                tcpClient.Dispose();
                return false;
            }
        }

        tcp = tcpClient;
        cts = new CancellationTokenSource();
        NetworkStream stream = tcpClient.GetStream();
        CancellationToken token = cts.Token;

        outbound = new ActionBlock<IMessage>(
            message =>
            {
                if (message is NetworkMessage networkMessage)
                    stream.Write(networkMessage.Serialize());
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, CancellationToken = token });

        // Inbound: a receive loop will parse frames and update the lobby/game state. It is
        // intentionally not started yet because NetworkMessageFactory.FromStream is still a
        // stub (returns null); starting it now would spin. It lands with the message layer.

        return true;
    }

    /// <summary>Queues a message to be sent to the server. Safe to call from the game thread.</summary>
    public void Send(IMessage message) => outbound?.Post(message);

    public void Disconnect()
    {
        if (Interlocked.Exchange(ref disconnected, 1) != 0) return;
        outbound?.Complete();
        try { cts?.Cancel(); } catch { /* already disposed */ }
        try { tcp?.Close(); } catch { /* already closed */ }
    }

    public void Dispose()
    {
        Disconnect();
        cts?.Dispose();
        tcp?.Dispose();
    }
}