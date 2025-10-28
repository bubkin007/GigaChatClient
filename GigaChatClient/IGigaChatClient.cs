using GigaChatClient.Models;

namespace GigaChatClient;

public interface IGigaChatClient
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    IReadOnlyCollection<string> GetAvailableModels();

    Task<string?> AskAsync(string prompt, CancellationToken cancellationToken = default);

    Task<string?> AskWithHistoryAsync(string userText, CancellationToken cancellationToken = default);

    void ResetHistory();

    Task<TokenResponse> RefreshTokenAsync(CancellationToken cancellationToken = default);

    Task<ModelListResponse?> GetModelsAsync(CancellationToken cancellationToken = default);

    Task<ModelResponse?> GetModelAsync(string modelId, CancellationToken cancellationToken = default);

    Task<ChatCompletionResponse?> ChatAsync(ChatRequest requestPayload, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TokensCountItem>> CountTokensAsync(TokensCountRequest requestPayload, CancellationToken cancellationToken = default);

    Task<BalanceResponse?> GetBalanceAsync(CancellationToken cancellationToken = default);

    Task<FileListResponse?> GetFilesAsync(CancellationToken cancellationToken = default);

    Task<FileDescription?> GetFileAsync(string fileId, CancellationToken cancellationToken = default);

    Task<FileDescription?> UploadFileAsync(Stream fileStream, string fileName, string purpose = FilePurpose.General, CancellationToken cancellationToken = default);

    Task<Stream> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default);

    Task<FileDeletedResponse?> DeleteFileAsync(string fileId, CancellationToken cancellationToken = default);

    Task<AiCheckResponse?> AiCheckAsync(AiCheckRequest requestPayload, CancellationToken cancellationToken = default);

    Task<EmbeddingsResponse?> CreateEmbeddingsAsync(EmbeddingsRequest requestPayload, CancellationToken cancellationToken = default);

    Task<BatchesListResponse?> GetBatchesAsync(string? batchId = null, CancellationToken cancellationToken = default);

    Task<BatchResponse?> CreateBatchAsync(Stream jsonlStream, string method, CancellationToken cancellationToken = default);

    Task<FunctionValidationResult?> ValidateFunctionAsync(CustomFunctionDescription description, CancellationToken cancellationToken = default);

    GigaChatOptions Options { get; }
}
