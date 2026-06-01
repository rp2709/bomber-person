using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BomberPerson.Core.Messages;
using BomberPerson.Core.State.NetworkMessages;

namespace BomberPerson.Core.Server;

public class Server(int port, long address)
{
    CancellationTokenSource cts;
    private Task task;
    private BroadcastBlock<IMessage> broadcastBlock;

    public async Task Run()
    {
        // construct server dataflow pipeline
        var messageBuffer = new BufferBlock<IMessage>();
        var simulationBlock = new TransformManyBlock<IMessage,IMessage>(new Simulation(new State.State()).ProcessMessage);
        broadcastBlock = new BroadcastBlock<IMessage>(message => message);
        var feedbackBlock = new RealisationDateBlock<IMessage>(FeedBackMessage.GetRealisationDate);

        messageBuffer.LinkTo(simulationBlock, new DataflowLinkOptions{ PropagateCompletion = true });
        simulationBlock.LinkTo(broadcastBlock, new DataflowLinkOptions{ PropagateCompletion =  true},message => message is NetworkMessage);
        simulationBlock.LinkTo(feedbackBlock, new DataflowLinkOptions{ PropagateCompletion = true },message => message is FeedBackMessage);
        feedbackBlock.LinkTo(simulationBlock);

        // Player slots 0..MaxPlayers-1; a slot returns to the pool when its player leaves.
        var freeSlots = new ConcurrentQueue<int>(Enumerable.Range(0, State.State.MaxPlayers));

        using TcpListener listener = new TcpListener(new IPEndPoint(new IPAddress(address), port));
        listener.Start();
        try
        {
            while (!cts.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(cts.Token);

                if (!freeSlots.TryDequeue(out int slot))
                {
                    client.GetStream().Write(new LobbyFull().Serialize());
                    client.Close();
                    continue;
                }

                _ = Task.Run(new ClientHandler(client, slot, messageBuffer, broadcastBlock,
                    () => freeSlots.Enqueue(slot), cts.Token).Handle);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal exit
        }
        finally
        {
            listener.Stop();
        }
    }

    public void RunAsync()
    {
        cts?.Cancel();
        try
        {
            task?.Wait();
        }
        catch (AggregateException)
        {
            // Ignore cancellation exception from the task
        }
        cts = new();
        task = Task.Run(Run);
    }

    public void RequestStop()
    {
        broadcastBlock?.Post(new ServerStoppingMessage());
        cts?.Cancel();
        try
        {
            task?.Wait();
        }
        catch (AggregateException)
        {
            // Ignore cancellation exception
        }
    }
}