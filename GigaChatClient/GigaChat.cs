using System.Net.Http.Json;

public class GigaChat
{
    private long _expiresAt;
    private readonly HttpClient _httpClient;
    private Token _token;
    private readonly List<string> _availableModelList;
    private readonly List<string> _roleList;
    private readonly string _secretKey;
    private readonly string _apiUrl = "https://gigachat.devices.sberbank.ru/api/v1/";
    private readonly string _apiUrlV1Models = "https://gigachat.devices.sberbank.ru/api/v1/models";
    private readonly string _oauthApiUrl = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
    private readonly string _apiDialogUrl = "https://gigachat.devices.sberbank.ru/api/v1/chat/completions";
    private List<Message> _sessionhistory;
    private string _gigachatmodel = "GigaChat";

    public GigaChat(HttpClient httpClient, string secretKey)
    {
        _httpClient = httpClient;
        _secretKey = secretKey;
        GetAccessToken();
        _expiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + _token.ExpiresAt;
        _availableModelList = GetModels().Result;
        _roleList = ["system", "user", "assistant", "function"];
        _sessionhistory = [];
    }

    private async Task<List<string>> GetModels()
    {
        try
        {
            var response = await _httpClient.SendAsync(CreateModelRequest());
            response.EnsureSuccessStatusCode();
            var modelsAnswer = await response.Content.ReadFromJsonAsync<GigaChatModels>();
            return modelsAnswer?.data.ConvertAll(model => model.id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving models: {ex.Message}");
            return null;
        }
    }
    private void ValidateToken()
    {
        if (_expiresAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            GetAccessToken();
        }
    }
    private HttpRequestMessage CreateModelRequest()
    {
        HttpRequestMessage request = new(HttpMethod.Get, _apiUrlV1Models);
        request.Headers.Add("Authorization", "Bearer " + _token.AccessToken);
        return request;
    }

    private void GetAccessToken()
    {
        try
        {
            var response = _httpClient.Send(ConfigAccessTokenRequest());
            _token = response.Content.ReadFromJsonAsync<Token>().Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating token: {ex.Message}");
        }
    }
    private HttpRequestMessage ConfigAccessTokenRequest()
    {
        var scope = "GIGACHAT_API_PERS";
        // var corpscope = "GIGACHAT_API_CORP";
        var formData = new List<KeyValuePair<string, string>>
        {
            new("scope", scope)
        };
        HttpRequestMessage request = new(HttpMethod.Post, _oauthApiUrl)
        {
            Content = new FormUrlEncodedContent(formData)
        };
        request.Headers.Add("Authorization", "Bearer " + _secretKey);
        request.Headers.Add("RqUID", Guid.NewGuid().ToString());
        return request;
    }

    public string SendDialog(List<Message> giga)
    {
        try
        {
            ChatRequest gigachatRequest = new ChatRequest() { model = _gigachatmodel, messages = giga };
            var response = _httpClient.Send(PrepareDialogRequest(gigachatRequest));
            var GigaChatAnswer = response.Content.ReadFromJsonAsync<GigaChatResult>().Result;
            return GigaChatAnswer.choices[0].message.content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating token: {ex.Message}");
        }
        return null;
    }
    public string BlindQuestion(string blindtext)
    {
        try
        {
            var blindGigaQuestion = new ChatRequest()
            {
                model = _gigachatmodel,
                messages = [new Message() { role = "user", content = blindtext }]
            };
            var response = _httpClient.Send(PrepareDialogRequest(blindGigaQuestion));
            var GigaChatResult = response.Content.ReadFromJsonAsync<GigaChatResult>().Result;
            return GigaChatResult.choices[0].message.content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating token: {ex.Message}");
        }
        return null;
    }
    private HttpRequestMessage PrepareDialogRequest(ChatRequest Giga)
    {
        HttpRequestMessage request = new(HttpMethod.Post, _apiDialogUrl)
        {
            Content = JsonContent.Create(Giga)
        };
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Authorization", "Bearer " + _token.AccessToken);
        return request;
    }
    public string AskWithHistory(string usertext)
    {
        _sessionhistory.Add(new Message() { role = "user", content = usertext });
        var gigachatext = SendDialog(_sessionhistory);
        _sessionhistory.Add(new Message() { role = "assistant", content = gigachatext });
        return gigachatext;
    }
}

