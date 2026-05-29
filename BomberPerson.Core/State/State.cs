using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BomberPerson.Core.State;

public interface IReadOnlyState
{
    State.Phase CurrentPhase { get; }
    IReadOnlyList<IReadOnlyPlayer> Players { get; }
    IReadOnlyList<Bomb> Bombs { get; }
    Terrain Terrain { get; }
    int CountDownValue { get; }
    DateTimeOffset Timestamp { get; }
}

public class State : IReadOnlyState
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
    IReadOnlyList<IReadOnlyPlayer> IReadOnlyState.Players => Players;
    public List<Bomb> Bombs { get; } = new();
    IReadOnlyList<Bomb> IReadOnlyState.Bombs => Bombs;
    public Terrain Terrain { get; set; }= new();
    public int CountDownValue { get; set; } = -1;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;

    public byte[] Encode()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write(Timestamp.ToUnixTimeMilliseconds());
        writer.Write((int)CurrentPhase);
        
        writer.Write(Players.Count);
        foreach (var player in Players)
        {
            player.Encode(writer);
        }

        writer.Write(Bombs.Count);
        foreach (var bomb in Bombs)
        {
            bomb.Encode(writer);
        }

        var terrainData = Terrain.Encode();
        writer.Write(terrainData.Length);
        writer.Write(terrainData);
        writer.Write(CountDownValue);

        return ms.ToArray();
    }

    public static State Decode(Stream stream)
    {
        var state = new State();
        using var reader = new BinaryReader(stream, System.Text.Encoding.Default, true);
        state.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64());
        state.CurrentPhase = (Phase)reader.ReadInt32();

        int playerCount = reader.ReadInt32();
        state.Players.Clear();
        for (int i = 0; i < playerCount; i++)
        {
            state.Players.Add(Player.Decode(reader));
        }

        int bombCount = reader.ReadInt32();
        state.Bombs.Clear();
        for (int i = 0; i < bombCount; i++)
        {
            state.Bombs.Add(Bomb.Decode(reader));
        }

        int terrainDataLength = reader.ReadInt32();
        byte[] terrainData = reader.ReadBytes(terrainDataLength);
        using var terrainMs = new MemoryStream(terrainData);
        state.Terrain = Terrain.Decode(terrainMs);
        state.CountDownValue = reader.ReadInt32();
        return state;
    }

    public State Clone()
    {
        var clone = new State
        {
            CurrentPhase = CurrentPhase,
            Terrain = Terrain.Clone(),
            CountDownValue = CountDownValue,
            Timestamp = Timestamp
        };
        foreach (var player in Players)
        {
            clone.Players.Add(player.Clone());
        }
        foreach (var bomb in Bombs)
        {
            clone.Bombs.Add(bomb.Clone());
        }
        return clone;
    }
}