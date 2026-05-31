using System;
using System.Collections.Generic;
using System.IO;

namespace BomberPerson.Core.State;

public interface IReadOnlyState
{
    State.Phase CurrentPhase { get; }
    IReadOnlyList<IReadOnlyPlayer> Players { get; }
    IReadOnlyList<Bomb> Bombs { get; }
    IReadOnlyList<Explosion> Explosions { get; }
    Terrain Terrain { get; }
    int CountDownValue { get; }
    DateTimeOffset CurrentDate { get; }

    DateTimeOffset GameStartDate { get;}

    int WinnerSlotId { get; }

    State Clone();
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

    public Player GetPlayer(int playerId)
    {
        return Players.Find(player => player.Number == playerId);
    }

    public Phase CurrentPhase { get; set; } =  Phase.Lobby;

    public List<Player> Players { get; }= new();
    IReadOnlyList<IReadOnlyPlayer> IReadOnlyState.Players => Players;
    public List<Bomb> Bombs { get; } = new();
    IReadOnlyList<Bomb> IReadOnlyState.Bombs => Bombs;
    public List<Explosion> Explosions { get; } = new();
    IReadOnlyList<Explosion> IReadOnlyState.Explosions => Explosions;
    public Terrain Terrain { get; set; } = Terrain.Generate();
    public int CountDownValue { get; set; } = -1;
    public DateTimeOffset CurrentDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset GameStartDate { get; set; } = DateTimeOffset.Now;
    public int WinnerSlotId { get; set; } = -1;

    public byte[] Encode()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write(CurrentDate.ToUnixTimeMilliseconds());
        writer.Write(GameStartDate.ToUnixTimeMilliseconds());
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

        writer.Write(Explosions.Count);
        foreach (var explosion in Explosions)
        {
            explosion.Encode(writer);
        }

        var terrainData = Terrain.Encode();
        writer.Write(terrainData.Length);
        writer.Write(terrainData);
        writer.Write(CountDownValue);
        writer.Write(WinnerSlotId);

        return ms.ToArray();
    }

    public static State Decode(Stream stream)
    {
        var state = new State();
        using var reader = new BinaryReader(stream, System.Text.Encoding.Default, true);
        state.CurrentDate = DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64());
        state.GameStartDate = DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64());
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

        int explosionCount = reader.ReadInt32();
        state.Explosions.Clear();
        for (int i = 0; i < explosionCount; i++)
        {
            state.Explosions.Add(Explosion.Decode(reader));
        }

        int terrainDataLength = reader.ReadInt32();
        byte[] terrainData = reader.ReadBytes(terrainDataLength);
        using var terrainMs = new MemoryStream(terrainData);
        state.Terrain = Terrain.Decode(terrainMs);
        state.CountDownValue = reader.ReadInt32();
        state.WinnerSlotId = reader.ReadInt32();
        return state;
    }

    public State Clone()
    {
        var clone = new State
        {
            CurrentPhase = CurrentPhase,
            Terrain = Terrain.Clone(),
            CountDownValue = CountDownValue,
            CurrentDate = CurrentDate,
            GameStartDate = GameStartDate,
            WinnerSlotId = WinnerSlotId,
        };
        foreach (var player in Players)
        {
            clone.Players.Add(player.Clone());
        }
        foreach (var bomb in Bombs)
        {
            clone.Bombs.Add(bomb.Clone());
        }
        foreach (var explosion in Explosions)
        {
            clone.Explosions.Add(explosion.Clone());
        }
        return clone;
    }
}