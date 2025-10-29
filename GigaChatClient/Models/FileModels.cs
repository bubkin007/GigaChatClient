using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GigaChatClient.Models;

public static class FilePurpose
{
    public const string General = "general";
}

public sealed class FileDescription
{
    [JsonPropertyName("bytes")]
    public long Bytes { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("filename")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = FilePurpose.General;

    [JsonPropertyName("access_policy")]
    public string AccessPolicy { get; set; } = "private";
}

public sealed class FileListResponse
{
    [JsonPropertyName("data")]
    public List<FileDescription> Data { get; set; } = [];
}

public sealed class FileDeletedResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }

    [JsonPropertyName("access_policy")]
    public string AccessPolicy { get; set; } = string.Empty;
}
