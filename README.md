# GigaChatClient

Модуль `GigaChatClient` — это высокоуровневая .NET-библиотека для подключения финтех-систем к платформе GigaChat. Клиент обеспечивает безопасную работу с OAuth, управление диалоговыми сессиями, обработку файлов и выполнение функций, что позволяет быстро запускать продукты на базе генеративного ИИ: от консультационных чат-ботов до сценариев скоринга и детектирования мошенничества.

## Архитектура решения
- **Бэкенд**: C# 12 / .NET 8, `HttpClient` с тонкой настройкой, строгая типизация моделей, гибкая загрузка конфигураций (код + `gigachatsettings.json`).
- **Фронтенд**: адаптивный HTML/CSS, лёгкий JS-клиент для взаимодействия с API вашего сервиса (напр., `fetch` в SPA/SSR). В примерах используется модульный подход с минифицированными конструкциями и синтаксическим сахаром.
- **Инфраструктура**: GitHub Packages/NuGet, GitHub Actions для CI/CD, защищённые секреты, обязательная проверка безопасности поставщиков.

## Бизнес-контекст и целевые сценарии
- Персональные финансовые ассистенты (чаты по продуктам, динамическая персонализация). 
- Поддержка операционистов: генерация ответов, поиск документов, структурирование заявок.
- KYC/KYB и комплаенс: анализ предоставленных данных, формирование пояснений по риск-профилю.
- Службы мониторинга транзакций: быстрые проверки, сценарии расследований, подготовка отчётности.

## Требования
| Компонент            | Версия            | Назначение |
|---------------------|-------------------|------------|
| .NET SDK            | 8.0+              | Сборка и публикация библиотеки |
| C#                  | 12                | Актуальные языковые возможности |
| Node.js (опционально)| 20+              | Сборка фронтенд-примеров |
| Git                 | 2.40+             | Управление версиями и релизами |
| OpenSSL/Cert-Store  | -                 | Импорт доверенных корневых сертификатов |

## Установка
### Через NuGet
```bash
 dotnet add package GigaChatClient
```

### Из исходников
```bash
 git clone https://github.com/<org>/GigaChatClient.git
 cd GigaChatClient
 dotnet build GigaChatClient/GigaChatClient.csproj -c Release
```
Пакет (nupkg) можно найти в `GigaChatClient/bin/Release`.

## Конфигурация доступа
Библиотека сочетает жёстко заданные параметры и данные из конфигурационного файла:
1. Передайте `authorizationKey` в коде при создании клиента.
2. (Опционально) создайте `gigachatsettings.json` рядом с исполняемым файлом.

```json
{
  "authorizationKey": "<секрет>",
  "scope": "GIGACHAT_API_PERS",
  "apiBaseAddress": "https://gigachat.devices.sberbank.ru/api/v1/",
  "oAuthEndpoint": "https://ngw.devices.sberbank.ru:9443/api/v2/oauth",
  "defaultModel": "GigaChat"
}
```
Если файл найден и корректно распарсен, значения будут дополнены и переопределены; при отсутствии файла используются параметры, указанные в коде.

## Быстрый старт
```csharp
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};
var http = new HttpClient(handler)
{
    Timeout = TimeSpan.FromSeconds(120)
};
var options = GigaChatOptionsLoader.Create("<ваш OAuth ключ>");
var gigaChat = await GigaChat.CreateAsync(http, options);
var reply = await gigaChat.AskAsync("Подбери ИИС с минимальным риском");
```

### Управление историей диалога
```csharp
var answer = await gigaChat.AskWithHistoryAsync("Сформируй отчёт по тратам за неделю");
if (string.IsNullOrWhiteSpace(answer))
{
    gigaChat.ResetHistory();
}
```

### Получение данных и аналитика
```csharp
var models = await gigaChat.GetModelsAsync();
var balance = await gigaChat.GetBalanceAsync();
var tokens = await gigaChat.CountTokensAsync(new TokensCountRequest { Input = new() { "Пример" } });
```

### Управление файлами и батчами
```csharp
await using var source = File.OpenRead("dataset.jsonl");
var uploaded = await gigaChat.UploadFileAsync(source, "dataset.jsonl");
var batch = await gigaChat.CreateBatchAsync(source, "POST /embeddings");
```

### Пользовательские функции
```csharp
var description = new CustomFunctionDescription
{
    Name = "calculate_payment",
    Description = "Ануитетный расчёт",
    Parameters = new FunctionParameters
    {
        Type = "object",
        Properties = new Dictionary<string, FunctionProperty>
        {
            ["principal"] = new FunctionProperty { Type = "number", Description = "Сумма кредита" },
            ["rate"] = new FunctionProperty { Type = "number", Description = "Годовая ставка" },
            ["months"] = new FunctionProperty { Type = "integer", Description = "Срок" }
        },
        Required = new List<string> { "principal", "rate", "months" }
    }
};
var validation = await gigaChat.ValidateFunctionAsync(description);
```

## Описание работы
### Поток авторизации и диалогов
1. Бэкенд проверяет наличие `gigachatsettings.json`, дополняет жёстко заданную конфигурацию и создаёт `GigaChatOptions` с обязательным `authorizationKey`.
2. `GigaChat.CreateAsync` инициализирует HTTP-клиент, получает OAuth-токен, сохраняет срок действия для внешнего мониторинга и подготавливает рабочую сессию.
3. Каждое обращение из фронтенда к вашему API преобразуется в `ChatRequest`, который передаётся через `AskWithHistoryAsync`, формируя устойчивый диалог.
4. Ответы нормализуются в `ChatCompletionResponse` и возвращаются на фронтенд, где минифицированный JS обновляет UI и запускает бизнес-процессы (например, скоринг, подготовка офферов).

### Обработка файлов и батчей
1. Клиент загружает документы или датасеты через `UploadFileAsync`, получая `FileDescription` с метаданными для дальнейшего трекинга.
2. При необходимости массовых расчётов формируется `CreateBatchAsync`, возвращающий `BatchResponse`, который отслеживается до завершения.
3. Полученные результаты сопоставляются с транзакционными данными, чтобы автоматизировать андеррайтинг и AML-проверки.

### Управление лимитами и токенами
1. Функции `GetBalanceAsync` и `CountTokensAsync` анализируют использование токенов по каждому сценарию.
2. Полученная аналитика обогащает отчётность, помогает распределять бюджеты между направлениями (ретейл, МСБ, private banking) и поддерживает SLA команд внедрения.

## Интеграция с фронтендом
1. Создайте бэкенд-метод, который получает текст пользователя и вызывает `gigaChat.AskWithHistoryAsync`.
2. На фронтенде используйте модульный JS:
   ```html
   <form id="chat"><input name="prompt" placeholder="Вопрос"/><button>Спросить</button></form>
   <section id="dialog"></section>
   <script type="module">
     const chat = document.querySelector('#chat');
     const dialog = document.querySelector('#dialog');
     chat.addEventListener('submit', async e => {
       e.preventDefault();
       const data = new FormData(chat);
       const prompt = data.get('prompt')?.toString().trim();
       if (!prompt) return;
       const response = await fetch('/api/chat', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ prompt }) });
       const payload = await response.json();
       dialog.insertAdjacentHTML('beforeend', `<article>${payload.answer}</article>`);
     });
   </script>
   ```
3. Обеспечьте хранение истории на стороне сервера для комплаенса и расследований.
4. Подключите аналитические события (например, метрики CSAT, FCR) для оценки качества ответов.

## Модели данных
| Файл | Класс | Назначение |
|------|-------|------------|
| `Models/ChatModels.cs` | `ChatMessage`, `ChatRequest`, `ChatCompletionResponse` | Представление диалоговых сообщений, опций генерации и ответов |
| `Models/TokenResponse.cs` | `TokenResponse` | Структура OAuth-токена, срок действия, тип токена |
| `Models/ModelModels.cs` | `ModelListResponse`, `ModelResponse` | Управление доступными моделями GigaChat |
| `Models/TokensCountModels.cs` | `TokensCountRequest`, `TokensCountItem` | Подсчёт токенов для оценки стоимости |
| `Models/BalanceModels.cs` | `BalanceResponse` | Данные баланса и лимитов |
| `Models/FileModels.cs` | `FileDescription`, `FileListResponse`, `FileDeletedResponse` | Операции с файлами и датасетами |
| `Models/EmbeddingsModels.cs` | `EmbeddingsRequest`, `EmbeddingsResponse` | Создание эмбеддингов для поиска и скоринга |
| `Models/BatchModels.cs` | `BatchResponse`, `BatchesListResponse` | Пакетная обработка запросов |
| `Models/AiCheckModels.cs` | `AiCheckRequest`, `AiCheckResponse` | Проверки AI Safety и комплаенса |
| `Models/FunctionModels.cs` | `CustomFunctionDescription`, `FunctionParameters`, `FunctionProperty` | Декларативное описание функций для вызова из GigaChat |
| `Models/GigaChatConfigurationModel.cs` | `GigaChatConfigurationModel` | Конфигурация для загрузки из JSON-файла |

## Рекомендованные бизнес-процессы
1. **Онбординг клиента**: сохраняйте факт выдачи токена, периодически обновляйте через `RefreshTokenAsync`, логируйте срок истечения (внешняя система мониторинга).
2. **Финансовые консультации**: комбинируйте `AskWithHistoryAsync` и `UploadFileAsync` для подачи клиентских документов.
3. **Риск-аналитика**: используйте `CreateEmbeddingsAsync` и `CountTokensAsync` для индексирования и оценки сложных кейсов.
4. **Файл-центричное взаимодействие**: `GetFilesAsync`, `DownloadFileAsync`, `DeleteFileAsync` позволяют сопровождать кейс на всём жизненном цикле сделки.

## Релиз библиотеки на GitHub
1. Создайте ветку `release/x.y.z` от `main`, обновите версию в `GigaChatClient.csproj` и README.
2. Выполните `dotnet pack -c Release` и убедитесь, что пакет собирается без предупреждений.
3. Обновите `CHANGELOG.md` (создайте при необходимости) с кратким описанием изменений и влиянием на бизнес-процессы.
4. Запустите CI (GitHub Actions) и убедитесь в зелёных статусах (сборка, анализ, SAST, проверка зависимостей).
5. Подпишите тег `git tag -s vX.Y.Z` и отправьте `git push origin release/x.y.z --tags`.
6. Создайте Pull Request в `main`, получите апрувы ответственных за безопасность и архитектуру.
7. После мерджа оформите GitHub Release: добавьте описание, контрольные суммы, вложите `.nupkg` и артефакты документации.
8. Опубликуйте пакет в GitHub Packages/NuGet (`dotnet nuget push`).
9. Сообщите в внутренних каналах (например, runbook в Confluence) о новой версии, добавьте инструкции для продуктовых команд.

## Чек-лист перед публикацией
- [ ] Версия и зависимости обновлены.
- [ ] Настройки OAuth протестированы на стейджинге.
- [ ] Политики хранения истории чатов согласованы с юристами.
- [ ] Секреты размещены в GitHub Actions (`GIGACHAT_AUTHORIZATION_KEY`).
- [ ] Проведено ревью на соответствие внутренним стандартам безопасности.

## Диагностика и безопасность
- При ошибках 401 выполните `RefreshTokenAsync` и проверьте Scope.
- При ошибках SSL обновите хранилище сертификатов или настройте `HttpClientHandler`.
- Ограничьте доступ к истории диалогов RBAC-политиками.
- Настройте мониторинг аномалий (количество токенов, задержки, частота батчей).

## Часто задаваемые вопросы
**Можно ли использовать прокси?** Да, настройте `HttpClientHandler.Proxy` и передайте кастомный `HttpClient` в `GigaChat.CreateAsync`.

**Как хранить историю?** Используйте персистентное хранилище (PostgreSQL, MongoDB) с шифрованием, либо временные слоты в Redis для коротких сессий.

**Как тестировать без продовых данных?** Подмените `HttpMessageHandler` заглушкой и используйте тестовые ключи из песочницы.

---

Готово к использованию! Добавьте библиотеку в свой релизный пайплайн и обеспечьте контроль качества на каждом шаге.
