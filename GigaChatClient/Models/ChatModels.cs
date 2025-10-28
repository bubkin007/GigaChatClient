using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GigaChatClient.Models;

public static class ChatRole
{
    public const string System = "system";
    public const string User = "user";
    public const string Assistant = "assistant";
    public const string Function = "function";
    public const string FunctionInProgress = "function_in_progress";
}

public sealed class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = ChatRole.User;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("functions_state_id")]
    public string? FunctionsStateId { get; set; }

    [JsonPropertyName("attachments")]
    public List<string>? Attachments { get; set; }
}

public sealed class FunctionCallRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public sealed class ChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = new();

    [JsonPropertyName("function_call")]
    public object? FunctionCall { get; set; }

    [JsonPropertyName("functions")]
    public List<CustomFunctionDescription>? Functions { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("repetition_penalty")]
    public double? RepetitionPenalty { get; set; }

    [JsonPropertyName("update_interval")]
    public double? UpdateInterval { get; set; }
}

public sealed class ChatChoice
{
    [JsonPropertyName("message")]
    public ChatMessage? Message { get; set; }

    [JsonPropertyName("delta")]
    public ChatMessage? Delta { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

public sealed class UsageData
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("precached_prompt_tokens")]
    public int PrecachedPromptTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public sealed class ChatCompletionResponse
{
    [JsonPropertyName("choices")]
    public List<ChatChoice> Choices { get; set; } = new();

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("usage")]
    public UsageData? Usage { get; set; }
}
