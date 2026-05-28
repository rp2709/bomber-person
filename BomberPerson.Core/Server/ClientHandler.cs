using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;
using BomberPerson.Core.Messages;
using BomberPerson.Core.State.NetworkMessages;

namespace BomberPerson.Core.Server;

public class ClientHandler(TcpClient client,BufferBlock<IMessage> messageBuffer)
{
    public void Handle()
    {
        messageBuffer.Post(new NewPlayerMessage());
        while (client.Connected)
        {
            var message = NetworkMessageFactory.FromStream(client.GetStream());
            if (message is null) break; // FromStream is still a stub; avoid a hot null loop
            if (message is LeaveGameMessage)
            {
                client.Close();
                break;
            }
            messageBuffer.Post(message);
        }
        
        // send client disconnected message
    }
}