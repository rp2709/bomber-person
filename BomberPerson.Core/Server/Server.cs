using System.Drawing;
using BomberPerson.Core.State;
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
    private int playerCount = 0;
    public void Run()
    {
        // construct server dataflow pipeline
        var messageBuffer = new BufferBlock<IMessage>();
        var simulationBlock = new TransformManyBlock<IMessage,IMessage>(new Simulation(new State.State()).ProcessMessage);
        var broadcastBlock = new BroadcastBlock<IMessage>((state1 => state1));
        
        messageBuffer.LinkTo(simulationBlock, new DataflowLinkOptions());
        simulationBlock.LinkTo(broadcastBlock, new DataflowLinkOptions{ PropagateCompletion =  true},message => message is NetworkMessage);
        
        using TcpListener listener = new TcpListener(new IPEndPoint(new IPAddress(address), port));
        listener.Start();
        while (!cts.IsCancellationRequested)
        {
            var client = listener.AcceptTcpClient();

            if (playerCount>= State.State.MaxPlayers)
            {
                client.GetStream().Write(new LobbyFull().Serialize());
                client.Close();
            }
            playerCount++;
            Task.Run(new ClientHandler(client,messageBuffer).Handle);
        }
        
        listener.Stop();
    }

    public CancellationTokenSource RunAsync()
    {
        cts.Cancel();
        cts = new();
        Task.Run(Run);
        return cts;
    } 
}