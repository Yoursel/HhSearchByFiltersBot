using System.Net.Http.Json;
using HhBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HhBot.Infrastructure.Telegram;

public class TelegramNotificationSender(
    HttpClient httpClient,
    IOptions<TelegramOptions> options,
    ILogger<TelegramNotificationSender> logger)
    : INotificationSender
{
    private readonly TelegramOptions _options = options.Value;

    public async Task SendAsync(string message, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        logger.LogInformation(
            "Sending Telegram message. MessageLength: {MessageLength}",
            message.Length);

        var request = new
        {
            chat_id = _options.ChatId,
            text = message
        };

        using var response = await httpClient.PostAsJsonAsync(
            $"/bot{_options.BotToken}/sendMessage",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "Telegram API request failed. StatusCode: {StatusCode}",
                response.StatusCode);

            response.EnsureSuccessStatusCode();
        }

        logger.LogInformation("Telegram message sent successfully");
    }
}