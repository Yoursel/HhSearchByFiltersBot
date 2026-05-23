using HhBot.Application.Formatting;
using HhBot.Application.Interfaces;
using HhBot.Application.Matching;
using HhBot.Application.Options;
using HhBot.Application.UseCases;
using HhBot.DI;
using HhBot.Infrastructure.Hh;
using HhBot.Infrastructure.Hh.Options;
using HhBot.Infrastructure.Hh.Persistence;
using HhBot.Infrastructure.Telegram;
using HhBot.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddConfiguredOptions<HhApiOptions>(builder.Configuration, HhApiOptions.SectionName)
    .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "Hh:BaseUrl is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.UserAgent), "Hh:UserAgent is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId), "Hh:ClientId is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.ClientSecret), "Hh:ClientSecret is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.AccessToken), "Hh:AccessToken is required");

builder.Services
    .AddConfiguredOptions<TelegramOptions>(builder.Configuration, TelegramOptions.SectionName)
    .Validate(options => !string.IsNullOrWhiteSpace(options.BotToken), "Telegram:BotToken is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.ChatId), "Telegram:ChatId is required");

builder.Services
    .AddConfiguredOptions<SearchOptions>(builder.Configuration, SearchOptions.SectionName)
    .Validate(options => options.CheckIntervalMinutes > 0, "Search:CheckIntervalMinutes must be greater than 0")
    .Validate(options => options.MaxVacanciesPerRun is > 0 and <= 100,
        "Search:MaxVacanciesPerRun must be between 1 and 100")
    .Validate(options => new[] { "Any", "Remote", "Office", "Hybrid" }
        .Contains(options.WorkMode, StringComparer.OrdinalIgnoreCase),
        "Search:WorkMode must be one of: Any, Remote, Office, Hybrid")
    .Validate(options => options.Keywords.Length > 0 || options.SkillKeywords.Length > 0, "At least one keyword is required");

builder.Services
    .AddConfiguredOptions<PersistenceOptions>(builder.Configuration, PersistenceOptions.SectionName)
    .Validate(options => !string.IsNullOrWhiteSpace(options.SentVacanciesFilePath),
        "Persistence:SentVacanciesFilePath is required");

builder.Services.AddSingleton<ISentVacancyStore, JsonSentVacancyStore>();
builder.Services.AddSingleton<VacancyMessageFormatter>();
builder.Services.AddSingleton<VacancyMatcher>();
builder.Services.AddTransient<CheckVacanciesUseCase>();
builder.Services.AddHostedService<VacancyPollingWorker>();

builder.Services.AddHttpClient<INotificationSender, TelegramNotificationSender>(client =>
{
    client.BaseAddress = new Uri("https://api.telegram.org");
});

builder.Services.AddHttpClient<IVacancySearchClient, HhVacancyClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<HhApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddHttpClient<IVacancyDetailsClient, HhVacancyDetailsClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<HhApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

await builder.Build().RunAsync();
