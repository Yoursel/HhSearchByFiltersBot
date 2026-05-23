using System.Text.Json.Serialization;
using HhBot.Infrastructure.Hh.Dto;

namespace HhBot.Infrastructure.Hh;

public class HhVacancySearchResponse
{
    [JsonPropertyName("items")]
    public HhVacancyDto[] Items { get; init; } = [];

    [JsonPropertyName("found")]
    public int Found { get; init; }

    [JsonPropertyName("pages")]
    public int Pages { get; init; }

    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("per_page")]
    public int PerPage { get; init; }
}