namespace GigaChatClient;

public sealed class GigaChatOptions
{
    public required string AuthorizationKey { get; init; }

    public string Scope { get; init; } = "GIGACHAT_API_PERS";

    public Uri ApiBaseAddress { get; init; } = new("https://gigachat.devices.sberbank.ru/api/v1/");

    public Uri OAuthEndpoint { get; init; } = new("https://ngw.devices.sberbank.ru:9443/api/v2/oauth");

    public string DefaultModel { get; init; } = "GigaChat";
}
