using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GigaChatClient.Models;

public sealed class EmbeddingsRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public List<string> Input { get; set; } = [];
}

public sealed class EmbeddingUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }
}

public sealed class EmbeddingData
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("embedding")]
    public List<double> Embedding { get; set; } = [];

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("usage")]
    public EmbeddingUsage? Usage { get; set; }
}

public sealed class EmbeddingsResponse
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<EmbeddingData> Data { get; set; } = [];

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
}
