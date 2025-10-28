using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

public sealed class GigaChat
{
    private readonly HttpClient _httpClient;
    private readonly string _secretKey;
    private readonly string _model;
    private readonly Uri _modelsEndpoint = new("https://gigachat.devices.sberbank.ru/api/v1/models");
    private readonly Uri _dialogEndpoint = new("https://gigachat.devices.sberbank.ru/api/v1/chat/completions");
    private readonly Uri _oauthEndpoint = new("https://ngw.devices.sberbank.ru:9443/api/v2/oauth");
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    private Token? _token;
    private long _tokenExpiresAt;
    private IReadOnlyCollection<string> _availableModels = Array.Empty<string>();
    private readonly List<Message> _sessionHistory = new();

    private GigaChat(HttpClient httpClient, string secretKey, string model)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
        _model = string.IsNullOrWhiteSpace(model) ? "GigaChat" : model;
    }

    public static async Task<GigaChat> CreateAsync(HttpClient httpClient, string secretKey, string? model = null, CancellationToken cancellationToken = default)
    {
        var client = new GigaChat(httpClient, secretKey, model ?? "GigaChat");
        await client.RefreshTokenAsync(cancellationToken).ConfigureAwait(false);
        await client.LoadAvailableModelsAsync(cancellationToken).ConfigureAwait(false);
        return client;
    }

    public static GigaChat Create(HttpClient httpClient, string secretKey, string? model = null)
    {
        return CreateAsync(httpClient, secretKey, model).GetAwaiter().GetResult();
    }

    public IReadOnlyCollection<string> GetAvailableModels()
    {
        return _availableModels;
    }

    public async Task<string?> AskAsync(string prompt, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);
        var message = new Message { Role = "user", Content = prompt };
        var response = await SendDialogAsync([message], cancellationToken).ConfigureAwait(false);
        return response;
    }

    public async Task<string?> AskWithHistoryAsync(string userText, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userText);
        _sessionHistory.Add(new Message { Role = "user", Content = userText });
        var reply = await SendDialogAsync(_sessionHistory, cancellationToken).ConfigureAwait(false);
        if (reply != null)
        {
            _sessionHistory.Add(new Message { Role = "assistant", Content = reply });
        }
        return reply;
    }

    public void ResetHistory()
    {
        _sessionHistory.Clear();
    }

    private async Task<string?> SendDialogAsync(IReadOnlyList<Message> messages, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(messages);
        if (messages.Count == 0)
        {
            throw new ArgumentException("Conversation requires at least one message", nameof(messages));
        }
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        var requestPayload = new ChatRequest
        {
            Model = _model,
            Messages = messages.Select(message => new Message { Role = message.Role, Content = message.Content }).ToList()
        };
        using var request = PrepareDialogRequest(requestPayload);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GigaChatResult>(_serializerOptions, cancellationToken).ConfigureAwait(false);
        if (result?.Choices == null)
        {
            return null;
        }
        if (result.Choices.Count == 0)
        {
            return null;
        }
        var answer = result.Choices[0].Message?.Content;
        return answer;
    }

    private async Task LoadAvailableModelsAsync(CancellationToken cancellationToken)
    {
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = CreateModelRequest();
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var models = await response.Content.ReadFromJsonAsync<GigaChatModels>(_serializerOptions, cancellationToken).ConfigureAwait(false);
        if (models?.Data == null)
        {
            _availableModels = Array.Empty<string>();
            return;
        }
        var ids = models.Data
            .Select(model => model.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        _availableModels = ids;
    }

    private async Task EnsureValidTokenAsync(CancellationToken cancellationToken)
    {
        if (_token == null)
        {
            await RefreshTokenAsync(cancellationToken).ConfigureAwait(false);
            return;
        }
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now >= _tokenExpiresAt)
        {
            await RefreshTokenAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task RefreshTokenAsync(CancellationToken cancellationToken)
    {
        using var request = CreateAccessTokenRequest();
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<Token>(_serializerOptions, cancellationToken).ConfigureAwait(false);
        if (token == null)
        {
            throw new InvalidOperationException("Token response is empty");
        }
        _token = token;
        _tokenExpiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + token.ExpiresAt;
    }

    private HttpRequestMessage CreateModelRequest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _modelsEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", RequireAccessToken());
        return request;
    }

    private HttpRequestMessage PrepareDialogRequest(ChatRequest payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _dialogEndpoint)
        {
            Content = JsonContent.Create(payload, options: _serializerOptions)
        };
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", RequireAccessToken());
        return request;
    }

    private HttpRequestMessage CreateAccessTokenRequest()
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new("scope", "GIGACHAT_API_PERS")
        };
        var request = new HttpRequestMessage(HttpMethod.Post, _oauthEndpoint)
        {
            Content = new FormUrlEncodedContent(formData)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _secretKey);
        request.Headers.Add("RqUID", Guid.NewGuid().ToString());
        return request;
    }

    private string RequireAccessToken()
    {
        var token = _token?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Access token is missing");
        }
        return token;
    }
}
