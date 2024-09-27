var handler = new HttpClientHandler()
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};
var secretKey = "";
var httpClient = new HttpClient(handler);
var GigaChat = new GigaChat(httpClient, secretKey);
var answer = GigaChat.BlindQuestion("как дела?");
Console.WriteLine(answer);