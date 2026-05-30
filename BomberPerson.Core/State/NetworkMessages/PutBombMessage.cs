using System;
using System.IO;
using BomberPerson.Core.Messages;
using BomberPerson.Core.Server;

namespace BomberPerson.Core.State.NetworkMessages;

public class PutBombMessage() : NetworkMessage, ISimulationMessage
{
    private static readonly TimeSpan BombDelay = TimeSpan.FromSeconds(5);

    public override MessageType Type => MessageType.PutBomb;

    public void Process(State state)
    {
        var player = state.GetPlayer(SlotId);
        if (player == null) return;

        if (player.LastBombPlaced + TimeSpan.FromSeconds(Player.BombPlaceDelayS) > DateTimeOffset.Now)
        {
            return;
        }
        player.LastBombPlaced  = DateTimeOffset.Now;
        
        uint x = (uint)(player.Position.X / 32);
        uint y = (uint)(player.Position.Y / 32);

        state.Bombs.Add(new Bomb(DateTimeOffset.Now + BombDelay)
        {
            PositionX = x,
            PositionY = y
        });
    }
}
