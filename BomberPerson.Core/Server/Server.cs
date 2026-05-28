using BomberPerson.Core.State;
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

    public void Run()
    {
        // construct server dataflow pipeline
        var messageBuffer = new BufferBlock<IMessage>();
        var simulationBlock = new TransformManyBlock<IMessage,IMessage>(new Simulation(new State.State()).ProcessMessage);
        var broadcastBlock = new BroadcastBlock<IMessage>(message => message);

        messageBuffer.LinkTo(simulationBlock, new DataflowLinkOptions{ PropagateCompletion = true });
        simulationBlock.LinkTo(broadcastBlock, new DataflowLinkOptions{ PropagateCompletion =  true},message => message is NetworkMessage);

        // Player slots 0..MaxPlayers-1; a slot returns to the pool when its player leaves.
        var freeSlots = new ConcurrentQueue<int>(Enumerable.Range(0, State.State.MaxPlayers));

        using TcpListener listener = new TcpListener(new IPEndPoint(new IPAddress(address), port));
        listener.Start();
        while (!cts.IsCancellationRequested)
        {
            var client = listener.AcceptTcpClient();

            if (!freeSlots.TryDequeue(out int slot))
            {
                client.GetStream().Write(new LobbyFull().Serialize());
                client.Close();
                continue;
            }

            Task.Run(new ClientHandler(client, slot, messageBuffer, broadcastBlock,
                () => freeSlots.Enqueue(slot)).Handle);
        }

        listener.Stop();
    }

    public void RunAsync()
    {
        cts?.Cancel();
        task?.Wait();
        cts = new();
        task = Task.Run(Run);
    }

    public void RequestStop()
    {
        cts?.Cancel();
    }
}