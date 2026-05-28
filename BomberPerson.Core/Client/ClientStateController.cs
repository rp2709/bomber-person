using System;
using System.Threading.Tasks;
using BomberPerson.Core.Messages;
using BomberPerson.Core.Scene;
using BomberPerson.Core.State;
using BomberPerson.Core.State.NetworkMessages;

namespace BomberPerson.Core.Client;

public class ClientStateController
{
    private State.State _state = new();
    public IReadOnlyState State => _state;

    private Server.Server localServerInstance;
    private NetworkClient networkClient = null;
    private readonly SceneManager sceneManager;

    public ClientStateController(SceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
        GoToMainMenu();
    }

    public StatusMessage StatusMessage { get; private set; }

    public void OnMessageReceived(IMessage message)
    {
        if (message is NewStateMessage newState)
        {
            _state = newState.State;
        }
        else if (message is LobbyFull)
        {
            StatusMessage = new StatusMessage("Lobby is full", StatusMessage.ImportanceLevels.Error);
            networkClient?.Disconnect();
        }
    }

    public void GoToHostMenu()
    {
        StatusMessage = null;
        sceneManager.LoadScene(new HostGameScene(this));
    }

    public void GoToJoinMenu()
    {
        StatusMessage = null;
        sceneManager.LoadScene(new JoinGameScene(this));
    }

    public void GoToMainMenu()
    {
        StatusMessage = null;
        sceneManager.LoadScene(new MainMenuScene(this));
    }

    public async void HostGame(string gameName, int port, string password)
    {
        StatusMessage = new StatusMessage("Starting server...", StatusMessage.ImportanceLevels.Info);
        localServerInstance = new Server.Server(port, 0L);
        localServerInstance.RunAsync();

        await Task.Delay(500); // Give server time to start

        JoinGame("Host", "127.0.0.1", port, password);
    }

    public async void JoinGame(string playerName, string ip, int port, string password)
    {
        StatusMessage = new StatusMessage("Connecting...", StatusMessage.ImportanceLevels.Info);
        networkClient?.Dispose();
        networkClient = new NetworkClient();
        networkClient.MessageReceived += OnMessageReceived;

        bool connected = await networkClient.ConnectAsync(ip, port);
        if (connected)
        {
            StatusMessage = new StatusMessage("Connected", StatusMessage.ImportanceLevels.Success);
            networkClient.Send(new JoinRequestMessage(playerName));
            sceneManager.LoadScene(new LobbyScene(this));
        }
        else
        {
            StatusMessage = new StatusMessage("Connection failed", StatusMessage.ImportanceLevels.Error);
        }
    }

    public void SetReady()
    {
        networkClient?.Send(new FlipReadyMessage());
    }

    public void QuitLobby()
    {
        networkClient?.Send(new LeaveGameMessage());
        networkClient?.Disconnect();
        localServerInstance?.RequestStop();
        GoToMainMenu();
    }
}