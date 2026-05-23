using HhBot.Application.Options;
using HhBot.Domain;

namespace HhBot.Application.Interfaces;

public interface IVacancySearchClient
{
    Task<IReadOnlyList<Vacancy>> SearchAsync(
        SearchOptions searchOptions,
        CancellationToken cancellationToken);
}