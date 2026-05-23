namespace HhBot.Domain;

public sealed record Vacancy(
    string Id,
    string Title,
    string? EmployerName,
    string Url,
    DateTimeOffset? PublishedAt);