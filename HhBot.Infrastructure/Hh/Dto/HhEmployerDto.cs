using System.Text.Json.Serialization;

namespace HhBot.Infrastructure.Hh.Dto;

public class HhEmployerDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}