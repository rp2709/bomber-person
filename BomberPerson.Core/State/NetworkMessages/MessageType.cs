namespace BomberPerson.Core.State.NetworkMessages;

public enum MessageType : byte
{
    LobbyFull,
    NewState,
    LeaveGame,
    Move,
    Stop,
    PutBomb,
}
