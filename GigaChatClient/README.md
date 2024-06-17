# Начало работы
 Установить сертификаты в рут или добавить хендлер для пропуска ошибки безопасности
### Иницилизация:
```cs-sharp
var handler = new HttpClientHandler()
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};
var httpClient = new HttpClient(handler);
var GigaChat = new GigaChat(httpClient, secretKey);
```
### Получение токена:
```cs-sharp
Получение токена происходит при инициализации
```
### Отправка запроса
Отправка запроса с сохранением истории
```cs-sharp
AskWithHistory
```
Отправка запроса без истории
```cs-sharp
BlindQuestion
```

