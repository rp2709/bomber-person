using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;

namespace BomberPerson.Core.Server;

public class ClientHandler(TcpClient client,BufferBlock<Message> messageBuffer)
{
    public void Handle()
    {
        // send client joined message
        while (client.Connected)
        {
            // read message
            
            // if message is leave game : client.Close();
            // else put message in buffer
        }
        
        // send client disconnected message
    }
}