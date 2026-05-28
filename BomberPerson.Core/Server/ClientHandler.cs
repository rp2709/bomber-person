using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BomberPerson.Core.Net;

namespace BomberPerson.Core.Server;

/// <summary>
/// Bridges one TCP client to the server pipeline. Inbound: reads framed messages and posts
/// them, tagged with the player id, into the command buffer. Outbound: an
/// <see cref="ActionBlock{T}"/> with MaxDegreeOfParallelism = 1 is the single writer for this
/// socket, fed first the <see cref="Welcome"/> and then the broadcast snapshots.
/// </summary>
public sealed class ClientHandler(
    TcpClient client,
    int playerId,
    ITargetBlock<ServerCommand> commands,
    ISourceBlock<byte[]> broadcast,
    Action onExit)
{
    public async Task HandleAsync(CancellationToken token)
    {
        using TcpClient owned = client;
        client.NoDelay = true;
        NetworkStream stream = client.GetStream();

        ActionBlock<byte[]> outbound = new(
            payload => Protocol.WriteFrameAsync(stream, payload, token),
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, CancellationToken = token });

        // Welcome is queued before the broadcast link, so it is the first frame the client sees.
        outbound.Post(new Welcome(playerId).Serialize());
        using IDisposable link = broadcast.LinkTo(outbound);

        commands.Post(new PlayerJoined(playerId));
        try
        {
            while (!token.IsCancellationRequested)
            {
                byte[]? payload = await Protocol.ReadFrameAsync(stream, token);
                if (payload is null) break;                      // client closed the connection
                Message message = Message.Deserialize(payload);
                if (message is LeaveMessage) break;
                commands.Post(new PlayerInput(playerId, message));
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { /* socket reset or malformed frame -> drop the client */ }
        finally
        {
            commands.Post(new PlayerLeft(playerId));
            outbound.Complete();
            onExit();
        }
    }
}