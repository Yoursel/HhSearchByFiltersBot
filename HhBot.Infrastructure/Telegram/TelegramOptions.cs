namespace HhBot.Infrastructure.Telegram;

public class TelegramOptions
{
    public const string SectionName = "Telegram";

    public required string BotToken { get; init; }
    public required string ChatId { get; init; }
}