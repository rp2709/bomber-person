using System;
using System.Numerics;

namespace BomberPerson.Core.Server;

public class PlayerCollisionMessage(int playerSlot, DateTimeOffset realisationDate) : FeedBackMessage
{
    public int PlayerSlot { get; } = playerSlot;
    protected override DateTimeOffset GetRealisationDate() => realisationDate;

    public override void Process(State.State state)
    {
        var player = state.GetPlayer(PlayerSlot);
        if (player != null)
        {
            player.Velocity = Vector2.Zero;
        }
    }
}
