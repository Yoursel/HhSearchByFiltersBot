using System.Text.Json.Serialization;

namespace HhBot.Infrastructure.Hh.Dto;

public class HhVacancyDetailsDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}