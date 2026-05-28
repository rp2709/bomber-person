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

    public byte[] Encode()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
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

        return ms.ToArray();
    }

    public static State Decode(Stream stream)
    {
        var state = new State();
        using var reader = new BinaryReader(stream, System.Text.Encoding.Default, true);
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
        return state;
    }
}