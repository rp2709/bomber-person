using System.Threading;
using System.Threading.Tasks;
using BomberPerson.Core.Messages;
using GameServer = BomberPerson.Core.Server.Server;

namespace BomberPerson.Core.Network;

/// <summary>
/// Single entry point the UI uses for networking. On the host it owns both the
/// <see cref="GameServer"/> (running on a background task) and a local <see cref="Client"/>
/// connected to it over loopback: the host plays as an ordinary client that happens to talk
/// to a server living in its own process. Remote players only use the client side.
/// </summary>
public class NetworkManager
{
    private static NetworkManager instance;
    public static NetworkManager Instance => instance ??= new NetworkManager();
    private NetworkManager() { }

    private GameServer server;
    private CancellationTokenSource serverCts;
    private Client client;

    public bool IsServer => server != null;
    public bool IsConnected => client != null && client.IsConnected;

    /// <summary>Starts the authoritative server in this process. No-op if one is already running.</summary>
    public void StartServer(int port, string password)
    {
        if (server != null) return;
        server = new GameServer(port, 0L); // 0 = IPAddress.Any: reachable on loopback and LAN
        serverCts = server.RunAsync();
    }

    /// <summary>Connects the local client to a server (127.0.0.1 for the host, a LAN IP for others).</summary>
    public async Task<bool> ConnectAsync(string host, int port, string playerName, string password)
    {
        client = new Client();
        return await client.ConnectAsync(host, port);
    }

    public void Send(IMessage message) => client?.Send(message);

    public void Disconnect()
    {
        client?.Disconnect();
        client = null;
        serverCts?.Cancel();
        serverCts = null;
        server = null;
    }
}