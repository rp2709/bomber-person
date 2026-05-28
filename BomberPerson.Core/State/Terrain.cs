using System.Data;
using System.IO;

namespace BomberPerson.Core.State;

public class Terrain
{
    enum Type{None,Empty,Box,Solid,Border}

    private int width,height; // max 2^4 = 16
    private Type[] grid = [];
    
    /**
     * Encodes the grid as a simple stream of cell types encoded as a single byte
     * The first byte indicates the grid dimension (width and height) in a single byte (4 bit values)
     */
    public byte[] Encode()
    {
        byte[] buffer = new byte[width * height + 1];
        buffer[0] = (byte)((width << 4 | height & 0xF) & 0xFF);
        for (int i = 0; i < width * height; ++i)
        {
            buffer[1 + i] = (byte)grid[i];
        }
        return buffer;
    }

    public void Decode(Stream data)
    {
        int header =  data.ReadByte();
        if (header < 0) throw new DataException("Expecting valid encoded terrain");
        byte bHeader = (byte)header;
        byte height =  (byte)(bHeader & 0x0F);
        byte width = (byte)(bHeader >> 4);
        byte[] buffer = new byte[width * height];
        data.ReadExactly(buffer);
    }
}