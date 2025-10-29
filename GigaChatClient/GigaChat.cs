using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using GigaChatClient.Models;

namespace GigaChatClient;

public sealed class GigaChat : IGigaChatClient
{
    private readonly HttpClient _httpClient;
    private readonly GigaChatOptions _options;
    private readonly JsonSerializerOptions _serializerOptions;
    private static readonly ReadOnlyCollection<string> EmptyModels = new(Array.Empty<string>());
    private readonly List<ChatMessage> _sessionHistory = [];
    private ReadOnlyCollection<string> _availableModels = EmptyModels;
    private TokenResponse? _token;
    private DateTimeOffset _tokenExpiry;
    private bool _initialized;

    public GigaChat(HttpClient httpClient, GigaChatOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public static async Task<GigaChat> CreateAsync(HttpClient httpClient, GigaChatOptions options, CancellationToken cancellationToken = default)
    {
        var client = new GigaChat(httpClient, options);
        await client.InitializeAsync(cancellationToken).ConfigureAwait(false);
        return client;
    }

    public static GigaChat Create(HttpClient httpClient, GigaChatOptions options)
    {
        return CreateAsync(httpClient, options).GetAwaiter().GetResult();
    }

    public GigaChatOptions Options => _options;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }
        await RefreshTokenAsync(cancellationToken).ConfigureAwait(false);
        await LoadAvailableModelsAsync(cancellationToken).ConfigureAwait(false);
        _initialized = true;
    }

    public IReadOnlyCollection<string> GetAvailableModels()
    {
        return _availableModels;
    }

    public async Task<string?> AskAsync(string prompt, string role = ChatRole.User, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        var message = new ChatMessage { Role = role, Content = prompt };
        return await SendDialogAsync([message], cancellationToken).ConfigureAwait(false);
    }

    public async Task<string?> AskWithHistoryAsync(string userText, string role = ChatRole.User, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userText);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        _sessionHistory.Add(new ChatMessage { Role = role, Content = userText });
        var reply = await SendDialogAsync(_sessionHistory, cancellationToken).ConfigureAwait(false);
        if (reply == null)
        {
            return null;
        }
        _sessionHistory.Add(new ChatMessage { Role = ChatRole.Assistant, Content = reply });
        return reply;
    }

    public void ResetHistory()
    {
        _sessionHistory.Clear();
    }

    public async Task<TokenResponse> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        using var request = CreateAccessTokenRequest();
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(_serializerOptions, cancellationToken).ConfigureAwait(false);
        if (token == null)
        {
            throw new InvalidOperationException("Token response is empty");
        }
        _token = token;
        _tokenExpiry = DateTimeOffset.FromUnixTimeMilliseconds(token.ExpiresAt);
        return token;
    }

    public async Task<ModelListResponse?> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = CreateAuthorizedRequest(HttpMethod.Get, new Uri(_options.ApiBaseAddress, "models"));
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ModelListResponse>(_serializerOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ModelResponse?> GetModelAsync(string modelId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        var endpoint = new Uri(_options.ApiBaseAddress, $"models/{modelId}");
        using var request = CreateAuthorizedRequest(HttpMethod.Get, endpoint);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ModelResponse>(_serializerOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ChatCompletionResponse?> ChatAsync(ChatRequest requestPayload, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requestPayload);
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        requestPayload.Model = string.IsNullOrWhiteSpace(requestPayload.Model) ? _options.DefaultModel : requestPayload.Model;
        using var request = CreateAuthorizedRequest(HttpMethod.Post, new Uri(_options.ApiBaseAddress, "chat/completions"));
        request.Content = JsonContent.Create(requestPayload, options: _serializerOptions);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(_serializerOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<TokensCountItem>> CountTokensAsync(TokensCountRequest requestPayload, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requestPayload);
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = CreateAuthorizedRequest(HttpMethod.Post, new Uri(_options.ApiBaseAddress, "tokens/count"));
        request.Content = JsonContent.Create(requestPayload, options: _serializerOptions);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<TokensCountItem>>(_serializerOptions, cancellationToken).ConfigureAwait(false);
        if (result == null)
        {
            return Array.Empty<TokensCountItem>();
        }
        return result;
    }

    public async Task<BalanceResponse?> GetBalanceAsync(CancellationToken cancellationToken = default)
    {
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = CreateAuthorizedRequest(HttpMethod.Get, new Uri(_options.ApiBaseAddress, "balance"));
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BalanceResponse>(_serializerOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<FileListResponse?> GetFilesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = CreateAuthorizedRequest(HttpMethod.Get, new Uri(_options.ApiBaseAddress, "files"));
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FileListResponse>(_serializerOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<FileDescription?> GetFileAsync(string fileId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileId);
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        var endpoint = new Uri(_options.ApiBaseAddress, $"files/{fileId}");
        using var request = CreateAuthorizedRequest(HttpMethod.Get, endpoint);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FileDescription>(_serializerOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<FileDescription?> UploadFileAsync(Stream fileStream, string fileName, string purpose = FilePurpose.General, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        using var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        form.Add(fileContent, "file", fileName);
        form.Add(new StringContent(purpose), "purpose");
        using var request = CreateAuthorizedRequest(HttpMethod.Post, new Uri(_options.ApiBaseAddress, "files"));
        request.Content = form;
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FileDescription>(_serializerOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Stream> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileId);
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        var endpoint = new Uri(_options.ApiBaseAddress, $"files/{fileId}/content");
        using var request = CreateAuthorizedRequest(HttpMethod.Get, endpoint);
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<FileDeletedResponse?> DeleteFileAsync(string fileId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileId);
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        var endpoint = new Uri(_options.ApiBaseAddress, $"files/{fileId}/delete");
        using var request = CreateAuthorizedRequest(HttpMethod.Post, endpoint);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FileDeletedResponse>(_serializerOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AiCheckResponse?> AiCheckAsync(AiCheckRequest requestPayload, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requestPayload);
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = CreateAuthorizedRequest(HttpMethod.Post, new Uri(_options.ApiBaseAddress, "ai/check"));
        request.Content = JsonContent.Create(requestPayload, options: _serializerOptions);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AiCheckResponse>(_serializerOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<EmbeddingsResponse?> CreateEmbeddingsAsync(EmbeddingsRequest requestPayload, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requestPayload);
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = CreateAuthorizedRequest(HttpMethod.Post, new Uri(_options.ApiBaseAddress, "embeddings"));
        request.Content = JsonContent.Create(requestPayload, options: _serializerOptions);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EmbeddingsResponse>(_serializerOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<BatchesListResponse?> GetBatchesAsync(string? batchId = null, CancellationToken cancellationToken = default)
    {
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        var endpointBuilder = new UriBuilder(new Uri(_options.ApiBaseAddress, "batches"));
        if (!string.IsNullOrWhiteSpace(batchId))
        {
            endpointBuilder.Query = $"batch_id={Uri.EscapeDataString(batchId)}";
        }
        using var request = CreateAuthorizedRequest(HttpMethod.Get, endpointBuilder.Uri);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BatchesListResponse>(_serializerOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<BatchResponse?> CreateBatchAsync(Stream jsonlStream, string method, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(jsonlStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(method);
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        var endpoint = new Uri(_options.ApiBaseAddress, $"batches?method={Uri.EscapeDataString(method)}");
        using var content = new StreamContent(jsonlStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        using var request = CreateAuthorizedRequest(HttpMethod.Post, endpoint);
        request.Content = content;
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BatchResponse>(_serializerOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<FunctionValidationResult?> ValidateFunctionAsync(CustomFunctionDescription description, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(description);
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = CreateAuthorizedRequest(HttpMethod.Post, new Uri(_options.ApiBaseAddress, "functions/validate"));
        request.Content = JsonContent.Create(description, options: _serializerOptions);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FunctionValidationResult>(_serializerOptions, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string?> SendDialogAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(messages);
        if (messages.Count == 0)
        {
            throw new ArgumentException("Conversation requires at least one message", nameof(messages));
        }
        var clonedMessages = new List<ChatMessage>(messages.Count);
        for (var i = 0; i < messages.Count; i++)
        {
            var original = messages[i];
            var clone = new ChatMessage
            {
                Role = original.Role,
                Content = original.Content,
                FunctionsStateId = original.FunctionsStateId,
                Attachments = original.Attachments == null ? null : new List<string>(original.Attachments)
            };
            clonedMessages.Add(clone);
        }
        var requestPayload = new ChatRequest
        {
            Model = _options.DefaultModel,
            Messages = clonedMessages
        };
        var response = await ChatAsync(requestPayload, cancellationToken).ConfigureAwait(false);
        if (response?.Choices == null)
        {
            return null;
        }
        if (response.Choices.Count == 0)
        {
            return null;
        }
        var choice = response.Choices[0];
        var content = choice?.Message?.Content;
        if (string.IsNullOrEmpty(content))
        {
            return content;
        }
        var limit = _options.ResponseCharacterLimit;
        if (limit <= 0)
        {
            return content;
        }
        if (content.Length <= limit)
        {
            return content;
        }
        return content[..limit];
    }

    private async Task LoadAvailableModelsAsync(CancellationToken cancellationToken)
    {
        var modelsResponse = await GetModelsAsync(cancellationToken).ConfigureAwait(false);
        var models = modelsResponse?.Data;
        if (models == null)
        {
            _availableModels = EmptyModels;
            return;
        }
        var unique = new HashSet<string>(StringComparer.Ordinal);
        var ordered = new List<string>(models.Count);
        for (var i = 0; i < models.Count; i++)
        {
            var model = models[i];
            if (model == null)
            {
                continue;
            }
            var id = model.Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }
            var added = unique.Add(id);
            if (added)
            {
                ordered.Add(id);
            }
        }
        _availableModels = ordered.Count == 0 ? EmptyModels : new ReadOnlyCollection<string>(ordered);
    }

    private async Task EnsureValidTokenAsync(CancellationToken cancellationToken)
    {
        if (_token == null)
        {
            await RefreshTokenAsync(cancellationToken).ConfigureAwait(false);
            return;
        }
        var now = DateTimeOffset.UtcNow;
        if (now >= _tokenExpiry)
        {
            await RefreshTokenAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private HttpRequestMessage CreateAccessTokenRequest()
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["scope"] = _options.Scope
        });
        var request = new HttpRequestMessage(HttpMethod.Post, _options.OAuthEndpoint)
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _options.AuthorizationKey);
        request.Headers.Add("RqUID", Guid.NewGuid().ToString());
        return request;
    }

    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, Uri uri)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", RequireAccessToken());
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
