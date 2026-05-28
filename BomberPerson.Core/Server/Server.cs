using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BomberPerson.Core.Net;

namespace BomberPerson.Core.Server;

/// <summary>
/// Hosts the authoritative game. It binds on every interface so the host (via 127.0.0.1) and
/// remote players (via the LAN address) share one accept path. Pipeline:
/// commands -> Simulation (single-threaded) -> broadcast -> one writer per client.
/// </summary>
public sealed class Server(int port)
{
    private const int TicksPerSecond = 30;
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(1.0 / TicksPerSecond);
    private const float TickSeconds = 1f / TicksPerSecond;

    private CancellationTokenSource? cts;

    /// <summary>Starts the server on a background task. Cancel the returned source to stop it.</summary>
    public CancellationTokenSource Start()
    {
        cts?.Cancel();
        CancellationTokenSource newCts = new();
        cts = newCts;
        _ = Task.Run(() => RunAsync(newCts.Token));
        return newCts;
    }

    public async Task RunAsync(CancellationToken token)
    {
        // The Simulation transform defaults to MaxDegreeOfParallelism = 1: state is mutated by
        // a single thread, so no locks are needed anywhere in the pipeline.
        BufferBlock<ServerCommand> commands = new();
        TransformManyBlock<ServerCommand, byte[]> simulation =
            new(new Simulation(new State.State()).Process);
        BroadcastBlock<byte[]> broadcast = new(payload => payload);

        commands.LinkTo(simulation, new DataflowLinkOptions { PropagateCompletion = true });
        simulation.LinkTo(broadcast, new DataflowLinkOptions { PropagateCompletion = true });

        // Player slots 0..MaxPlayers-1; a slot returns to the pool when its player leaves.
        ConcurrentQueue<int> freeSlots = new(Enumerable.Range(0, State.State.MaxPlayers));

        _ = Task.Run(() => TickLoopAsync(commands, token), CancellationToken.None);

        using TcpListener listener = new(IPAddress.Any, port);
        listener.Start();
        try
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(token);
                if (!freeSlots.TryDequeue(out int playerId))
                {
                    _ = RejectAsync(client, RejectReason.ServerFull);
                    continue;
                }

                ClientHandler handler = new(client, playerId, commands, broadcast,
                    () => freeSlots.Enqueue(playerId));
                _ = Task.Run(() => handler.HandleAsync(token), CancellationToken.None);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            listener.Stop();
            commands.Complete();
        }
    }

    private static async Task TickLoopAsync(ITargetBlock<ServerCommand> commands, CancellationToken token)
    {
        using PeriodicTimer timer = new(TickInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(token))
                commands.Post(new Tick(TickSeconds));
        }
        catch (OperationCanceledException) { }
    }

    private static async Task RejectAsync(TcpClient client, RejectReason reason)
    {
        try
        {
            using (client)
                await Protocol.WriteFrameAsync(client.GetStream(), new Rejected(reason).Serialize());
        }
        catch { /* client already gone */ }
    }
}