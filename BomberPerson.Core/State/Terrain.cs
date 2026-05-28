using System.IO;

namespace BomberPerson.Core.State;

public class Terrain
{
    public Terrain(uint width, uint height)
    {
        if (width < 1 || height < 1)
            throw new System.ArgumentException("Terrain width and height must be greater than zero");
        Width = width;
        Height = height;
        grid = new Type[Width * Height];
    }

    public Terrain()
    {
        Width = 16;
        Height = 16;
        grid = new Type[Width * Height];
    }
    public enum Type{None,Empty,Box,Solid,Border}

    public uint Width { get; private set; }
    public uint Height{get; private set; }
    private Type[] grid;
    public Type this[uint x, uint y]
    {
        get => grid[y * Width + x];
        set => grid[y * Width + x] = value;
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

    public void Decode(Stream data)
    {
        using var reader = new BinaryReader(data, System.Text.Encoding.Default, true);
        Width = reader.ReadUInt32();
        Height = reader.ReadUInt32();
        grid = new Type[Width * Height];
        for (int i = 0; i < Width * Height; ++i)
        {
            grid[i] = (Type)reader.ReadByte();
        }
    }
}