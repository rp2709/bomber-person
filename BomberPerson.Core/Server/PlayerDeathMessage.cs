using System;
using BomberPerson.Core.Messages;

namespace BomberPerson.Core.Server;

public class PlayerDeathMessage(int playerSlot, DateTimeOffset realisationDate) : FeedBackMessage
{
    public int PlayerSlot { get; } = playerSlot;
    private readonly DateTimeOffset _realisationDate = realisationDate;

    protected override DateTimeOffset GetRealisationDate() => _realisationDate;

    public override void Process(State.State state)
    {
        var player = state.GetPlayer(PlayerSlot);
        if (player != null)
        {
            player.IsAlive = false;
            player.Velocity = System.Numerics.Vector2.Zero;
        }
    }
}
