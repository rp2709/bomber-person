using System.Drawing;

namespace BomberPerson.Core;

public static class Settings
{
    public const int TileSize = 32;
    public const int PlayerRadius = 10;
    public const int BombRadius = 12;
    public const int BombExplosionDelayMilliseconds = 2500;
    public const int BombExplosionDurationMilliseconds = 1500;
    public const uint BombPlaceDelayS = 3;
    
    public const uint TerrainDefaultWidth = 20;
    public const uint TerrainDefaultHeight = 20;
    
    public static readonly Color[] Palette =
    [
        Color.FromArgb(30,  60,  150),
        Color.FromArgb(200, 50,  50),
        Color.FromArgb(60,  180, 70),
        Color.FromArgb(220, 200, 40)
    ];
    
    public const int MaxPlayers = 4;
}