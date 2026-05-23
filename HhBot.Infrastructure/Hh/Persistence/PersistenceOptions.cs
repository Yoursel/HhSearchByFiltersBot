namespace HhBot.Infrastructure.Hh.Persistence;

public class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public string SentVacanciesFilePath { get; init; } = "sent-vacancies.json";
}