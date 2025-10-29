using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GigaChatClient.Models;

public sealed class BalanceItem
{
    [JsonPropertyName("usage")]
    public string Usage { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public int Value { get; set; }
}

public sealed class BalanceResponse
{
    [JsonPropertyName("balance")]
    public List<BalanceItem> Balance { get; set; } = [];
}
