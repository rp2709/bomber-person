using System.Net.Sockets;
using System.Text.Json.Serialization;

namespace BomberPerson.Core.Lobby;

public record NetworkPlayer(
    [property: JsonPropertyName("id")]      int    Id,
    [property: JsonPropertyName("name")]    string Name,
    [property: JsonPropertyName("isHost")]  bool   IsHost,
    [property: JsonPropertyName("isReady")] bool   IsReady
);