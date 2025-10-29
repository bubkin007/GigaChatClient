using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GigaChatClient.Models;

public sealed class ModelResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("owned_by")]
    public string OwnedBy { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public sealed class ModelListResponse
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<ModelResponse> Data { get; set; } = [];
}
