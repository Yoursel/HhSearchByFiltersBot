namespace HhBot.Infrastructure.Hh.Persistence;

public sealed record SentVacancyRecord(
    string VacancyId,
    string Title,
    string? EmployerName,
    string Url,
    DateTimeOffset SentAt);