using BomberPerson.Core.Messages;
using BomberPerson.Core.Scene;

namespace BomberPerson.Core.Client;

public class ClientStateController(SceneManager sceneManager)
{
    private State.State  state = new();
    private Server.Server localServerInstance;
    private NetworkClient networkClient;
    private StatusMessage statusMessage;

    public void OnMessageReceived(IMessage message)
    {
        
    }

    public void GoToHostMenu()
    {
        sceneManager.LoadScene(new HostGameScene(this));
    }

    public void GoToJoinMenu()
    {
        sceneManager.LoadScene(new JoinGameScene(this));
    }

    public void GoToMainMenu()
    {
        sceneManager.LoadScene(new MainMenuScene(this));
    }

    public void HostGame(string gameName, int port, string password)
    {
        localServerInstance = new Server.Server(port, 0L);
        localServerInstance.RunAsync();
    }

    public void JoinGame(string playerName, string ip, int port, string password)
    {
        // construct new game client and assign it
    }

    public void SetReady(bool ready)
    {
        
    }

    public void QuitLobby()
    {
        
    }

    public void QuitGame()
    {
        
    }
}