namespace BomberPerson.Core.State.NetworkMessages;

public enum MessageType : byte
{
    LobbyFull = 1,
    NewState = 2,
    LeaveGame = 3,
    SetReady = 4,
}
