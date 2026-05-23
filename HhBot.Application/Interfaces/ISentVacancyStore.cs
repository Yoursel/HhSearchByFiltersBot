using HhBot.Domain;

namespace HhBot.Application.Interfaces;

public interface ISentVacancyStore
{
    Task<IReadOnlySet<string>> GetSentVacancyIdsAsync(
        IReadOnlyCollection<string> vacancyIds,
        CancellationToken cancellationToken);

    Task MarkAsSentAsync(
        Vacancy vacancy,
        CancellationToken cancellationToken);
}