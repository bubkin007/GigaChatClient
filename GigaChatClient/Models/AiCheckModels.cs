using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GigaChatClient.Models;

public sealed class AiCheckRequest
{
    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
}

public sealed class AiCheckResponse
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("characters")]
    public int Characters { get; set; }

    [JsonPropertyName("tokens")]
    public int Tokens { get; set; }

    [JsonPropertyName("ai_intervals")]
    public List<List<int>> AiIntervals { get; set; } = [];
}
