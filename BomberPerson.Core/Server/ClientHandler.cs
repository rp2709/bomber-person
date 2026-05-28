using System;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;
using BomberPerson.Core.Messages;
using BomberPerson.Core.State.NetworkMessages;

namespace BomberPerson.Core.Server;

/// <summary>
/// Bridges one TCP client to the server pipeline.
/// Inbound: reads framed wire messages, tags them with this connection's slot, and posts them
/// into the shared buffer. Outbound: an ActionBlock (single writer) subscribed to the broadcast
/// writes every state snapshot back to this client.
/// </summary>
public class ClientHandler(
    TcpClient client,
    int slot,
    BufferBlock<IMessage> messageBuffer,
    ISourceBlock<IMessage> broadcast,
    Action onExit)
{
    public void Handle()
    {
        NetworkStream stream = client.GetStream();

        ActionBlock<IMessage> outbound = new(
            message =>
            {
                if (message is NetworkMessage networkMessage)
                {
                    networkMessage.SlotId = slot;
                    try { stream.Write(networkMessage.Serialize()); }
                    catch { /* socket gone; the read loop will tear things down */ }
                }
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });

        // Subscribe before announcing the join, so the resulting snapshot reaches this client.
        using IDisposable link = broadcast.LinkTo(outbound);

        messageBuffer.Post(new NewPlayerMessage(slot));

        try
        {
            while (client.Connected)
            {
                IMessage message = NetworkMessageFactory.FromStream(stream,slot);
                if (message is null or LeaveGameMessage) break;                 // peer disconnected or leaving
                messageBuffer.Post(message);
            }
        }
        catch { /* socket reset or malformed frame */ }
        finally
        {
            messageBuffer.Post(new PlayerLeftMessage(slot));
            outbound.Complete();
            client.Close();
            onExit();
        }
    }
}