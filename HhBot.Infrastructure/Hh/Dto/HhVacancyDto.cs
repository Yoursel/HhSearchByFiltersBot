using System.Text.Json.Serialization;

namespace HhBot.Infrastructure.Hh.Dto;

public class HhVacancyDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("alternate_url")]
    public string? AlternateUrl { get; init; }

    [JsonPropertyName("published_at")]
    public string? PublishedAt { get; init; }

    [JsonPropertyName("employer")]
    public HhEmployerDto? Employer { get; init; }
}