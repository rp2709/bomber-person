using System.Collections.Generic;

namespace BomberPerson.Core.State;

public class State
{
    public const int MaxPlayers = 4;
    public enum Phase
    {
        Lobby,
        Game,
        EndGame,
    }

    public Phase CurrentPhase { get; set; } =  Phase.Lobby;

    public List<Player> Players { get; }= new();
    public Terrain Terrain { get; }= new();
}