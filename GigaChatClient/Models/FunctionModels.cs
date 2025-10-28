using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GigaChatClient.Models;

public sealed class CustomFunctionExample
{
    [JsonPropertyName("request")]
    public string Request { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public Dictionary<string, object?> Params { get; set; } = new();
}

public sealed class CustomFunctionDescription
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("parameters")]
    public Dictionary<string, object?> Parameters { get; set; } = new();

    [JsonPropertyName("few_shot_examples")]
    public List<CustomFunctionExample>? FewShotExamples { get; set; }

    [JsonPropertyName("return_parameters")]
    public Dictionary<string, object?>? ReturnParameters { get; set; }
}

public sealed class FunctionIssue
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("schema_location")]
    public string SchemaLocation { get; set; } = string.Empty;
}

public sealed class FunctionValidationResult
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("json_ai_rules_version")]
    public string? JsonAiRulesVersion { get; set; }

    [JsonPropertyName("errors")]
    public List<FunctionIssue>? Errors { get; set; }

    [JsonPropertyName("warnings")]
    public List<FunctionIssue>? Warnings { get; set; }
}
