namespace HhBot.Infrastructure.Hh.Options;

public class HhApiOptions
{
    public const string SectionName = "Hh";

    public required string BaseUrl { get; init; }
    public required string UserAgent { get; init; }
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string AccessToken { get; init; }
}