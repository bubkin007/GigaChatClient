using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GigaChatClient.Models;

public sealed class BatchRequestCounts
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("completed")]
    public int Completed { get; set; }

    [JsonPropertyName("failed")]
    public int Failed { get; set; }
}

public sealed class BatchItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("request_counts")]
    public BatchRequestCounts RequestCounts { get; set; } = new();

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("output_file_id")]
    public string? OutputFileId { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }
}

public sealed class BatchesListResponse
{
    [JsonPropertyName("batches")]
    public List<BatchItem> Batches { get; set; } = [];
}

public sealed class BatchResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("request_counts")]
    public BatchRequestCounts RequestCounts { get; set; } = new();

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }
}
