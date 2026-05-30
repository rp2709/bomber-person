using System;
using System.Threading;
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
    public int? MySlotId { get; private set; }

    private Server.Server localServerInstance;
    private NetworkClient networkClient = null;
    private readonly SceneManager sceneManager;

    public ClientStateController(SceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
        GoToMainMenu();
    }

    private StatusMessage _statusMessage;
    private CancellationTokenSource _statusCancellationTokenSource;

    public StatusMessage StatusMessage
    {
        get => _statusMessage;
        private set
        {
            _statusMessage = value;
            _statusCancellationTokenSource?.Cancel();
            _statusCancellationTokenSource?.Dispose();
            _statusCancellationTokenSource = null;

            if (value != null)
            {
                _statusCancellationTokenSource = new CancellationTokenSource();
                _ = ClearStatusAfterDelay(_statusCancellationTokenSource.Token);
            }
        }
    }

    private async Task ClearStatusAfterDelay(CancellationToken token)
    {
        try
        {
            await Task.Delay(3000, token);
            _statusMessage = null;
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation
        }
    }

    public void OnMessageReceived(IMessage message)
    {
        if (message is NetworkMessage networkMessage)
        {
            MySlotId = networkMessage.SlotId;
        }

        if (message is NewStateMessage newState)
        {
            if (_state.CurrentPhase != newState.State.CurrentPhase)
            {
                switch (newState.State.CurrentPhase)
                {
                    case Core.State.State.Phase.Lobby : 
                        GoToLobby();
                        break;
                    case Core.State.State.Phase.Game: 
                        GoToGame();
                        break;
                    case Core.State.State.Phase.EndGame:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            _state = newState.State;
        }
        else if (message is LobbyFull)
        {
            StatusMessage = new StatusMessage("Lobby is full", StatusMessage.ImportanceLevels.Error);
            networkClient?.Disconnect();
        }
        else if (message is ServerStoppingMessage)
        {
            StatusMessage = new StatusMessage("Server is stopping", StatusMessage.ImportanceLevels.Info);
            _state = new();
            QuitLobby();
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
            GoToLobby();
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

    public void Move(MoveMessage.MoveDirection direction)
    {
        networkClient?.Send(new MoveMessage(direction));
    }

    public void Stop()
    {
        networkClient?.Send(new StopMessage());
    }

    public void PutBomb()
    {
        networkClient?.Send(new PutBombMessage());
    }

    private void GoToLobby()
    {
        sceneManager.LoadScene(new LobbyScene(this));
    }

    private void GoToGame()
    {
        sceneManager.LoadScene(new GameScene(this));
    }

    public void QuitLobby()
    {
        networkClient?.Send(new LeaveGameMessage());
        networkClient?.Disconnect();
        localServerInstance?.RequestStop();
        GoToMainMenu();
    }
}