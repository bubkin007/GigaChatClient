using System.Text.Json.Serialization;

namespace GigaChatClient.Models;

public sealed class GigaChatConfigurationModel
{
    [JsonPropertyName("authorizationKey")]
    public string? AuthorizationKey { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("apiBaseAddress")]
    public string? ApiBaseAddress { get; set; }

    [JsonPropertyName("oauthEndpoint")]
    public string? OAuthEndpoint { get; set; }

    [JsonPropertyName("defaultModel")]
    public string? DefaultModel { get; set; }

    [JsonPropertyName("responseCharacterLimit")]
    public int? ResponseCharacterLimit { get; set; }
}
