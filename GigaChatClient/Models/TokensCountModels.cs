using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GigaChatClient.Models;

public sealed class TokensCountRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public List<string> Input { get; set; } = new();
}

public sealed class TokensCountItem
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("tokens")]
    public int Tokens { get; set; }

    [JsonPropertyName("characters")]
    public int Characters { get; set; }
}
