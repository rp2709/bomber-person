using BomberPerson.Core.Net;

namespace BomberPerson.Core.Server;

/// <summary>
/// Internal command processed by the single-threaded <see cref="Simulation"/>. Unlike the raw
/// wire <see cref="Message"/>, it carries the sender's identity, and includes the synthetic
/// lifecycle and clock events (<see cref="PlayerJoined"/>, <see cref="PlayerLeft"/>,
/// <see cref="Tick"/>) so the simulation is the single source of truth for game state.
/// </summary>
public abstract class ServerCommand { }

public sealed class PlayerJoined(int playerId) : ServerCommand
{
    public int PlayerId { get; } = playerId;
}

public sealed class PlayerLeft(int playerId) : ServerCommand
{
    public int PlayerId { get; } = playerId;
}

public sealed class PlayerInput(int playerId, Message message) : ServerCommand
{
    public int PlayerId { get; } = playerId;
    public Message Message { get; } = message;
}

public sealed class Tick(float deltaSeconds) : ServerCommand
{
    public float DeltaSeconds { get; } = deltaSeconds;
}