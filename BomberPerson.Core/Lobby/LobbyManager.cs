using System.Collections.Generic;
using BomberPerson.Core.Scene;

namespace BomberPerson.Core.Lobby;

public class LobbyManager
{
    private static LobbyManager instance;
    public static LobbyManager Instance => instance ??= new LobbyManager();
    private LobbyManager() { }

    public LobbyScene          Scene        { get; private set; }
    public List<NetworkPlayer> Players      { get; private set; } = new List<NetworkPlayer>();
    public bool                GameStarting { get; private set; } = false;
    public string              ErrorMessage { get; private set; } = "";
    public bool                HostLeft     { get; private set; } = false;

    public void SetLobby(LobbyScene scene, string lobbyName)
    {
        Scene = scene;
        scene.SetName(lobbyName);
    }
    
    public void SetState(List<NetworkPlayer> players)
    {
        Players      = players;
        ErrorMessage = "";
    }

    public void StartGame()
    {
        GameStarting = true;
    }

    public void OnHostDisconnected()
    {
        HostLeft = true;
    }

    public void OnError(string message)
    {
        ErrorMessage = message;
    }

    public void Reset()
    {
        Players      = new List<NetworkPlayer>();
        GameStarting = false;
        ErrorMessage = "";
        HostLeft     = false;
    }
}