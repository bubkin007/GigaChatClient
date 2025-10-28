using System.Net.Http;
using GigaChatClient;

var factory = new GigaChatClientFactory(() =>
{
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    return new HttpClient(handler, true);
});

var secretKey = "";
var options = GigaChatOptionsLoader.Create(secretKey);
var firstClient = await factory.CreateAsync(options);
var secondClient = await factory.CreateAsync(options);
var answer = await firstClient.AskAsync("как дела?");
Console.WriteLine(answer);
