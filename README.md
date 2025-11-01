# GigaChatClient

Высокоуровневая .NET-библиотека, которая соединяет финтех-платформы с GigaChat и обеспечивает защищённые сценарии консультаций, скоринга, антифрода и аналитики. Клиент сочетает строгую типизацию, двойной контроль конфигурации (код и `gigachatsettings.json`), а также интегрируется с фронтендом на HTML/CSS + модульном JavaScript.

## Содержание
1. [Ценности для финтех-команд](#ценности-для-финтех-команд)
2. [Архитектура и стек](#архитектура-и-стек)
3. [Установка и запуск](#установка-и-запуск)
4. [Конфигурация доступа](#конфигурация-доступа)
5. [Модели данных](#модели-данных)
6. [Ключевые бизнес-процессы](#ключевые-бизнес-процессы)
7. [Интеграция с API и фронтендом](#интеграция-с-api-и-фронтендом)
8. [Потоки работы и мониторинг](#потоки-работы-и-мониторинг)
9. [Сборка, релизы и контроль качества](#сборка-релизы-и-контроль-качества)
10. [Частые вопросы](#частые-вопросы)

## Ценности для финтех-команд
- Быстрая реализация финансовых ассистентов: подключение чат-ботов, объяснимые рекомендации, подготовка офферов.
- Поддержка операционистов и колл-центров: генерация ответов, поиск документов, нормализация заявок.
- Риск-аналитика и AML: массовые проверки, работа с батчами, оценка лимитов и токенов.
- Подготовка отчётности: регулярные выгрузки данных, контроль SLA, аудит истории взаимодействий.

## Архитектура и стек
- **Бэкенд**: C# 12 / .NET 8, кастомный `HttpClient`, строгая типизация DTO, конфигурация через код и JSON.
- **Фронтенд**: адаптивный HTML/CSS, модульный ES-синтаксис, минифицированные обработчики событий, fetch API.
- **Инфраструктура**: GitHub Actions, NuGet/GitHub Packages, SAST и проверка поставщиков, хранение секретов в CI.

### Системные требования
| Компонент             | Версия | Назначение |
|-----------------------|--------|------------|
| .NET SDK              | 8.0+   | Сборка и тестирование библиотеки |
| C#                    | 12     | Современный синтаксис и паттерны |
| Node.js (опционально) | 20+    | Сборка фронтенд-примеров |
| Git                   | 2.40+  | Управление релизами |
| OpenSSL/Cert-Store    | —      | Импорт корневых сертификатов |

## Установка и запуск
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
Собранный пакет появится в `GigaChatClient/bin/Release`.

## Конфигурация доступа
1. Жёстко задайте минимум параметров в коде (например, `authorizationKey`, таймауты, целевую модель).
2. Добавьте `gigachatsettings.json` рядом с исполняемым файлом, чтобы переопределить значения при развертывании.

```json
{
  "authorizationKey": "<секрет>",
  "scope": "GIGACHAT_API_PERS",
  "apiBaseAddress": "https://gigachat.devices.sberbank.ru/api/v1/",
  "oAuthEndpoint": "https://ngw.devices.sberbank.ru:9443/api/v2/oauth",
  "defaultModel": "GigaChat"
}
```
Код инициализирует `GigaChatOptions`, затем проверяет наличие `gigachatsettings.json`. При успешном чтении параметры из файла дополняют или заменяют значения из кода, сохраняя двойной контроль доступа.

## Модели данных
| Файл | Классы | Назначение |
|------|--------|------------|
| `Models/ChatModels.cs` | `ChatMessage`, `ChatRequest`, `ChatCompletionResponse` | Диалоговые сообщения, параметры генерации, ответы модели |
| `Models/TokenResponse.cs` | `TokenResponse` | OAuth-токен, тип, срок действия |
| `Models/ModelModels.cs` | `ModelListResponse`, `ModelResponse` | Управление доступными моделями и выбором модели по сценарию |
| `Models/TokensCountModels.cs` | `TokensCountRequest`, `TokensCountItem` | Подсчёт токенов для бюджетирования |
| `Models/BalanceModels.cs` | `BalanceResponse` | Балансы и лимиты по продуктовым направлениям |
| `Models/FileModels.cs` | `FileDescription`, `FileListResponse`, `FileDeletedResponse` | Работа с документами и датасетами клиентов |
| `Models/EmbeddingsModels.cs` | `EmbeddingsRequest`, `EmbeddingsResponse` | Создание эмбеддингов для поиска транзакций и кейсов |
| `Models/BatchModels.cs` | `BatchResponse`, `BatchesListResponse` | Пакетные операции, верификация результатов |
| `Models/AiCheckModels.cs` | `AiCheckRequest`, `AiCheckResponse` | Проверки безопасности, политики использования ИИ |
| `Models/FunctionModels.cs` | `CustomFunctionDescription`, `FunctionParameters`, `FunctionProperty` | Декларативное описание бизнес-функций для вызова из GigaChat |
| `Models/GigaChatConfigurationModel.cs` | `GigaChatConfigurationModel` | Структура конфигурации для слияния кода и JSON |

## Ключевые бизнес-процессы
1. **Онбординг и доступ**: выпуск OAuth-ключа, настройка мониторинга срока действия, встраивание в runbook.
2. **Финансовые консультации**: `AskWithHistoryAsync` поддерживает устойчивый диалог, `UploadFileAsync` добавляет документы клиента.
3. **Операционный контроль**: `GetBalanceAsync` и `CountTokensAsync` дают метрики потребления по направлениям (ретейл, МСБ, private banking).
4. **Риск-аналитика и AML**: батчи (`CreateBatchAsync`) и эмбеддинги (`CreateEmbeddingsAsync`) ускоряют массовые проверки и расследования.
5. **Отчётность и SLA**: история запросов хранится в отдельном сервисе, что упрощает аудит и комплаенс.

## Интеграция с API и фронтендом
### Бэкенд на C#
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

var result = await gigaChat.AskWithHistoryAsync("Подбери ИИС с минимальным риском");
if (string.IsNullOrWhiteSpace(result))
{
    gigaChat.ResetHistory();
}
```

### Фронтенд на HTML/CSS + JS
```html
<form id="chat">
  <input name="prompt" placeholder="Вопрос" />
  <button>Спросить</button>
</form>
<section id="dialog"></section>
<script type="module">
  const chat = document.querySelector('#chat');
  const dialog = document.querySelector('#dialog');
  chat.addEventListener('submit', async e => {
    e.preventDefault();
    const prompt = new FormData(chat).get('prompt')?.toString().trim();
    if (!prompt) return;
    const response = await fetch('/api/chat', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ prompt }) });
    const payload = await response.json();
    dialog.insertAdjacentHTML('beforeend', `<article>${payload.answer}</article>`);
  });
</script>
```

## Потоки работы и мониторинг
1. Проверка `gigachatsettings.json`, слияние конфигурации, инициализация `GigaChatOptions`.
2. `GigaChat.CreateAsync` получает OAuth-токен, фиксирует срок действия, готовит сессию.
3. API-провайдер нормализует запросы в `ChatRequest`, сохраняет историю, применяет RBAC.
4. Метрики токенов, задержек и ошибок отправляются в мониторинг и SIEM.
5. Обработка файлов (`UploadFileAsync`, `DownloadFileAsync`, `DeleteFileAsync`) поддерживает цикл сделки и расследования.

## Сборка, релизы и контроль качества
1. Создайте ветку `release/x.y.z`, обновите версии и README.
2. Выполните `dotnet pack -c Release`, убедитесь в отсутствии предупреждений.
3. Обновите `CHANGELOG.md` и сохраните влияние на бизнес-процессы.
4. Пройдите CI (сборка, анализ, безопасность, проверка зависимостей).
5. Подпишите тег `vX.Y.Z`, создайте PR, получите апрувы безопасности и архитектуры.
6. Оформите GitHub Release, приложите артефакты и `.nupkg`.
7. Опубликуйте пакет в NuGet/GitHub Packages и уведомите продуктовые команды.

## Частые вопросы
**Можно ли использовать прокси?** Настройте `HttpClientHandler.Proxy` и передайте кастомный `HttpClient` в `GigaChat.CreateAsync`.

**Как хранить историю чатов?** Используйте защищённое хранилище (PostgreSQL, MongoDB) с шифрованием, либо временные слоты в Redis.

**Как тестировать без боевых данных?** Подмените `HttpMessageHandler` заглушкой, применяйте тестовые ключи песочницы.

---

Готово к внедрению: библиотека ускоряет запуск финтех-сценариев и поддерживает контроль качества на каждом этапе.
