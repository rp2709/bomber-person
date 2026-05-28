using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BomberPerson.Core.Messages;
using BomberPerson.Core.State.NetworkMessages;

namespace BomberPerson.Core.Client;

/// <summary>
/// Client-side TCP endpoint. The host (connecting to 127.0.0.1) and remote players use it
/// identically. Outbound messages go through an <see cref="ActionBlock{T}"/> with
/// MaxDegreeOfParallelism = 1, so the socket has a single writer and needs no locks.
/// </summary>
public class NetworkClient : IDisposable
{
    private TcpClient tcp;
    private ActionBlock<IMessage> outbound;
    private CancellationTokenSource cts;
    private int disconnected;

    public bool IsConnected => Volatile.Read(ref disconnected) == 0 && tcp != null && tcp.Connected;

    /// <summary>Raised on a background thread for every message received from the server.</summary>
    public event Action<IMessage> MessageReceived;

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

        // Inbound: parse messages off the socket and raise them. FromStream blocks until a full
        // message arrives (or the peer disconnects), so this loop does not spin.
        _ = Task.Run(() => ReceiveLoop(stream, token));

        return true;
    }

    /// <summary>Queues a message to be sent to the server. Safe to call from the game thread.</summary>
    public void Send(IMessage message) => outbound?.Post(message);

    private void ReceiveLoop(NetworkStream stream, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && tcp != null && tcp.Connected)
            {
                IMessage message = NetworkMessageFactory.FromStream(stream);
                if (message is null) break;            // server closed the connection
                MessageReceived?.Invoke(message);
            }
        }
        catch { /* socket reset or malformed frame -> treat as disconnect */ }
        finally { Disconnect(); }
    }

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