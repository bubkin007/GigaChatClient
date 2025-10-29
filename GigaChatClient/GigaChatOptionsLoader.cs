using System.Text.Json;

using GigaChatClient.Models;

namespace GigaChatClient;

public static class GigaChatOptionsLoader
{
    private const string DefaultConfigurationFileName = "gigachatsettings.json";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static GigaChatOptions Create(string authorizationKey, string? configurationFilePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authorizationKey);
        var baseOptions = new GigaChatOptions { AuthorizationKey = authorizationKey };
        var resolvedPath = ResolveConfigurationPath(configurationFilePath);
        if (!File.Exists(resolvedPath))
        {
            return baseOptions;
        }
        using var stream = File.OpenRead(resolvedPath);
        var fileOptions = JsonSerializer.Deserialize<GigaChatConfigurationModel>(stream, SerializerOptions);
        if (fileOptions == null)
        {
            return baseOptions;
        }
        var authorization = string.IsNullOrWhiteSpace(fileOptions.AuthorizationKey) ? baseOptions.AuthorizationKey : fileOptions.AuthorizationKey!;
        var scope = string.IsNullOrWhiteSpace(fileOptions.Scope) ? baseOptions.Scope : fileOptions.Scope!;
        var apiBaseAddress = SelectUri(fileOptions.ApiBaseAddress, baseOptions.ApiBaseAddress);
        var oauthEndpoint = SelectUri(fileOptions.OAuthEndpoint, baseOptions.OAuthEndpoint);
        var defaultModel = string.IsNullOrWhiteSpace(fileOptions.DefaultModel) ? baseOptions.DefaultModel : fileOptions.DefaultModel!;
        var responseCharacterLimit = baseOptions.ResponseCharacterLimit;
        if (fileOptions.ResponseCharacterLimit.HasValue)
        {
            var limitCandidate = fileOptions.ResponseCharacterLimit.Value;
            if (limitCandidate > 0)
            {
                responseCharacterLimit = limitCandidate;
            }
        }
        return new GigaChatOptions
        {
            AuthorizationKey = authorization,
            Scope = scope,
            ApiBaseAddress = apiBaseAddress,
            OAuthEndpoint = oauthEndpoint,
            DefaultModel = defaultModel,
            ResponseCharacterLimit = responseCharacterLimit
        };
    }

    private static string ResolveConfigurationPath(string? configurationFilePath)
    {
        if (!string.IsNullOrWhiteSpace(configurationFilePath))
        {
            return configurationFilePath;
        }
        return Path.Combine(AppContext.BaseDirectory, DefaultConfigurationFileName);
    }

    private static Uri SelectUri(string? candidate, Uri fallback)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return fallback;
        }
        var success = Uri.TryCreate(candidate, UriKind.Absolute, out var uri);
        if (!success || uri == null)
        {
            return fallback;
        }
        return uri;
    }
}
