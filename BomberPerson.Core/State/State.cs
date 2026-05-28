using System.Collections.Generic;
using System.Drawing;

namespace BomberPerson.Core.State;

public class State
{
    public const int MaxPlayers = 4;

    private static readonly Color[] Palette =
        { Color.Red, Color.RoyalBlue, Color.LimeGreen, Color.Gold };

    public enum Phase
    {
        Lobby,
        Game,
        EndGame,
    }

    public Phase CurrentPhase { get; set; } =  Phase.Lobby;

    public List<Player> Players { get; }= new();
    public Terrain Terrain { get; }= new();

    public Player? FindPlayer(int number)
    {
        foreach (Player p in Players)
            if (p.Number == number) return p;
        return null;
    }

    public void AddPlayer(int number)
    {
        if (FindPlayer(number) is not null) return;
        Players.Add(new Player(number, Palette[number % Palette.Length]));
    }

    public void RemovePlayer(int number)
    {
        for (int i = 0; i < Players.Count; i++)
            if (Players[i].Number == number) { Players.RemoveAt(i); return; }
    }

    /// <summary>
    /// Serializes the full state for a snapshot. Single place to extend as the model grows
    /// (positions, bombs, ...). Layout: [ phase ][ playerCount ] then per player
    /// [ number ][ A ][ R ][ G ][ B ][ ready ], followed by the encoded terrain.
    /// </summary>
    public byte[] Encode()
    {
        List<byte> bytes = new() { (byte)CurrentPhase, (byte)Players.Count };
        foreach (Player p in Players)
        {
            bytes.Add((byte)p.Number);
            Color c = p.Color;
            bytes.Add(c.A);
            bytes.Add(c.R);
            bytes.Add(c.G);
            bytes.Add(c.B);
            bytes.Add((byte)(p.Ready ? 1 : 0));
        }
        bytes.AddRange(Terrain.Encode());
        return bytes.ToArray();
    }
}