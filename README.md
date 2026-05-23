# HhBot

Telegram-бот для мониторинга вакансий на hh.ru по пользовательским фильтрам.

Бот ищет вакансии через HeadHunter API по заданным фильтрам, добирает полное описание новых вакансий, отправляет подходящие вакансии в Telegram-чат и сохраняет обработанные `vacancyId` в локальный JSON-файл, чтобы не присылать повторы.

## Настройки

Несекретные настройки лежат в `HhBot/appsettings.json`.

Пример:

```json
{
  "Hh": {
    "BaseUrl": "https://api.hh.ru/",
    "UserAgent": "HhSearchByFiltersBot/1.0 (your-email@example.com)"
  },
  "Search": {
    "Keywords": ["C#", ".NET", "ASP.NET Core"],
    "SkillKeywords": ["SQL", "REST API"],
    "AreaIds": [],
    "WorkMode": "Remote",
    "ExperienceIds": ["between1And3", "between3And6"],
    "PublishedFrom": "2026-05-20",
    "CheckIntervalMinutes": 60,
    "MaxVacanciesPerRun": 10,
    "ExcludeKeywords": ["1C", "PHP", "Bitrix", "WordPress", "Unity", "GameDev", "Frontend", "QA"]
  },
  "Persistence": {
    "SentVacanciesFilePath": "sent-vacancies.json"
  }
}
```

Допустимые значения `WorkMode`:

- `Any`
- `Remote`
- `Office`
- `Hybrid`

## Секреты

Для локальной разработки используются `.NET user-secrets`.

Команды нужно выполнять из папки startup-проекта:

```powershell
cd .\HhBot
```

Инициализация user-secrets:

```powershell
dotnet user-secrets init
```

Telegram:

```powershell
dotnet user-secrets set "Telegram:BotToken" "your-telegram-bot-token"
dotnet user-secrets set "Telegram:ChatId" "your-telegram-chat-id"
```

HeadHunter:

```powershell
dotnet user-secrets set "Hh:ClientId" "your-hh-client-id"
dotnet user-secrets set "Hh:ClientSecret" "your-hh-client-secret"
dotnet user-secrets set "Hh:AccessToken" "your-hh-access-token"
```

### Как получить значения для HeadHunter

`ClientId` и `ClientSecret` берутся в личном кабинете разработчика HH:

1. Открой документацию HH API: <https://api.hh.ru/openapi/redoc>.
2. Перейди в раздел `OAuth`.
3. Войди в аккаунт HH.
4. Создай новое приложение или открой уже созданное.
5. Скопируй значения `Client Id` и `Client Secret`.

`ClientId` сохраняется так:

```powershell
dotnet user-secrets set "Hh:ClientId" "copied-client-id"
```

`ClientSecret` сохраняется так:

```powershell
dotnet user-secrets set "Hh:ClientSecret" "copied-client-secret"
```

`AccessToken` можно получить через OAuth flow приложения `client_credentials`.

В PowerShell:

```powershell
$clientId = "copied-client-id"
$clientSecret = "copied-client-secret"
$userAgent = "HhSearchByFiltersBot/1.0 (your-email@example.com)"

$response = Invoke-RestMethod `
  -Method Post `
  -Uri "https://api.hh.ru/token" `
  -ContentType "application/x-www-form-urlencoded" `
  -Headers @{ "HH-User-Agent" = $userAgent } `
  -Body @{
    grant_type = "client_credentials"
    client_id = $clientId
    client_secret = $clientSecret
  }

$response.access_token
```

После получения токена сохрани его:

```powershell
dotnet user-secrets set "Hh:AccessToken" "$($response.access_token)"
```

Если HH вернул ошибку `invalid_client`, проверь `ClientId` и `ClientSecret`.
Если HH вернул ошибку про `User-Agent`, замени `your-email@example.com` на реальную контактную почту.

Проверить сохраненные ключи:

```powershell
dotnet user-secrets list
```

Токены, `ChatId`, `ClientSecret`, `AccessToken` и локальные файлы состояния нельзя коммитить.

## Запуск

Проект таргетит `.NET 10`, поэтому нужен установленный `.NET 10 SDK`.

Запуск из папки startup-проекта:

```powershell
cd .\HhBot
$env:DOTNET_ENVIRONMENT = "Development"
dotnet run
```

В Rider в Run Configuration нужно добавить переменную окружения:

```text
DOTNET_ENVIRONMENT=Development
```

## Локальное состояние

Обработанные вакансии сохраняются в:

```text
sent-vacancies.json
```

Путь настраивается так:

```json
"Persistence": {
  "SentVacanciesFilePath": "sent-vacancies.json"
}
```

Если нужно повторно протестировать отправку тех же вакансий, можно удалить этот файл.

## Важные детали

- `UserAgent` для HH должен содержать название приложения и реальную контактную почту.
- Для ограничения даты публикации используется `date_from`.
- `period` не используется вместе с `date_from`.
- Для `Remote`, `Office`, `Hybrid` в HH отправляется параметр `work_format`.
- `ExcludeKeywords` отправляются в HH через `excluded_text`.
- Дополнительно вакансии фильтруются локально через `VacancyMatcher`.
- Вакансия сохраняется как обработанная только после успешной отправки в Telegram.
