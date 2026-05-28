using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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
    Action onExit,
    CancellationToken token)
{
    public async Task Handle()
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

        try
        {
            while (client.Connected && !token.IsCancellationRequested)
            {
                IMessage message = await NetworkMessageFactory.FromStreamAsync(stream, token, slot);
                if (message is null or LeaveGameMessage) break;                 // peer disconnected or leaving
                
                if (message is JoinRequestMessage joinRequest)
                {
                    messageBuffer.Post(new NewPlayerMessage(slot, joinRequest.PlayerName));
                }
                else
                {
                    messageBuffer.Post(message);
                }
            }
        }
        catch (OperationCanceledException) { /* server stopping */ }
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