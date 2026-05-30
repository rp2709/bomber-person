using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using BomberPerson.Core.Server;

namespace BomberPerson.Core.State;

public class Terrain
{
    public const int TileSize = 32;
    public const uint DefaultWidth = 21;
    public const uint DefaultHeight = 15;

    public Terrain(uint width, uint height)
    {
        if (width < 5 || height < 5)
            throw new ArgumentException("Terrain width and height must be at least 5");
        Width = width;
        Height = height;
        grid = new Type[Width * Height];
        playerStarts = new Vector2[State.MaxPlayers];
    }

    public Terrain() : this(DefaultWidth, DefaultHeight) { }

    public enum Type{None,Empty,Box,Solid,Border}

    public uint Width { get; private set; }
    public uint Height{get; private set; }
    private Type[] grid;
    private Vector2[] playerStarts;

    /// <summary>
    /// Pixel-space spawn positions, one per player slot. Computed at generation time and
    /// only meaningful on the server (clients receive player positions in the state stream).
    /// </summary>
    public IReadOnlyList<Vector2> PlayerStarts => playerStarts;

    public Type this[uint x, uint y]
    {
        get => grid[y * Width + x];
        set => grid[y * Width + x] = value;
    }

    /// <summary>
    /// Builds a playable terrain: indestructible border, interior filled with destructible
    /// boxes, three safe tiles around each corner spawn, and random gaps dug through the
    /// boxes so the map is traversable from the start.
    /// </summary>
    public static Terrain Generate(uint width = DefaultWidth, uint height = DefaultHeight, int? seed = null)
    {
        var terrain = new Terrain(width, height);
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();

        for (uint y = 0; y < height; y++)
        {
            for (uint x = 0; x < width; x++)
            {
                bool isBorder = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                terrain[x, y] = isBorder ? Type.Border : Type.Empty;
            }
        }

        var corners = new (uint x, uint y)[]
        {
            (1, 1),
            (width - 2, 1),
            (1, height - 2),
            (width - 2, height - 2),
        };

        var safe = new HashSet<(uint, uint)>();
        foreach (var (cx, cy) in corners)
        {
            safe.Add((cx, cy));
            uint adjX = cx == 1 ? cx + 1 : cx - 1;
            uint adjY = cy == 1 ? cy + 1 : cy - 1;
            safe.Add((adjX, cy));
            safe.Add((cx, adjY));
        }

        for (uint y = 1; y < height - 1; y++)
        {
            for (uint x = 1; x < width - 1; x++)
            {
                if (safe.Contains((x, y))) continue;
                terrain[x, y] = Type.Box;
            }
        }

        int interior = (int)((width - 2) * (height - 2));
        int holeCount = rng.Next(interior / 6, interior / 4 + 1);
        for (int i = 0; i < holeCount; i++)
        {
            uint x = (uint)rng.Next(1, (int)width - 1);
            uint y = (uint)rng.Next(1, (int)height - 1);
            if (terrain[x, y] == Type.Box)
                terrain[x, y] = Type.Empty;
        }

        for (int i = 0; i < corners.Length && i < terrain.playerStarts.Length; i++)
        {
            var (cx, cy) = corners[i];
            terrain.playerStarts[i] = new Vector2(cx * TileSize + (TileSize - Simulation.PlayerRadius * 2), cy * TileSize + (TileSize - Simulation.PlayerRadius * 2));
        }

        return terrain;
    }

    /**
     * Encodes the grid as a simple stream of cell types encoded as a single byte
     * The first bytes indicate the grid's dimensions (Width, Height as uint)
     */
    public byte[] Encode()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write(Width);
        writer.Write(Height);
        for (int i = 0; i < Width * Height; ++i)
        {
            writer.Write((byte)grid[i]);
        }
        return ms.ToArray();
    }

    public static Terrain Decode(Stream data)
    {
        using var reader = new BinaryReader(data, System.Text.Encoding.Default, true);
        var width = reader.ReadUInt32();
        var height = reader.ReadUInt32();
        var terrain = new Terrain(width, height);
        for (int i = 0; i < width * height; ++i)
        {
            terrain.grid[i] = (Type)reader.ReadByte();
        }
        return terrain;
    }

    public Terrain Clone()
    {
        var clone = new Terrain(Width, Height);
        Array.Copy(grid, clone.grid, grid.Length);
        Array.Copy(playerStarts, clone.playerStarts, playerStarts.Length);
        return clone;
    }
}