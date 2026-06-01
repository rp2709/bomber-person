using System;
using BomberPerson.Core.Server;

namespace BomberPerson.Core.State.NetworkMessages;

public class PutBombMessage : NetworkMessage, ISimulationMessage
{
    private static readonly TimeSpan BombDelay = TimeSpan.FromMilliseconds(Settings.BombExplosionDelayMilliseconds);

    public override MessageType Type => MessageType.PutBomb;

    public void Process(State state)
    {
        var player = state.GetPlayer(SlotId);
        if (player == null) return;

        if (player.LastBombPlaced + TimeSpan.FromSeconds(Settings.BombPlaceDelayS) > DateTimeOffset.Now)
        {
            return;
        }
        player.LastBombPlaced  = DateTimeOffset.Now;
        
        var x = (uint)(player.Position.X / Settings.TileSize);
        var y = (uint)(player.Position.Y / Settings.TileSize);

        state.Bombs.Add(new Bomb(DateTimeOffset.Now + BombDelay)
        {
            PositionX = x,
            PositionY = y
        });
    }
}
