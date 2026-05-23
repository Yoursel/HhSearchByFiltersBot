namespace HhBot.Application.Options;

public class SearchOptions
{
    public const string SectionName = "Search";

    public string[] Keywords { get; init; } = [];
    public string[] SkillKeywords { get; init; } = [];
    public string[] IncludeKeywords { get; init; } = [];
    public string[] AreaIds { get; init; } = [];
    public required string WorkMode { get; init; }
    public string[] ExperienceIds { get; init; } = [];
    public required DateOnly PublishedFrom { get; init; }
    public int CheckIntervalMinutes { get; init; }
    public int MaxVacanciesPerRun { get; init; } = 10;
    public string[] ExcludeKeywords { get; init; } = [];
}


