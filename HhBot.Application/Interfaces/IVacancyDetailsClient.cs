using HhBot.Domain;

namespace HhBot.Application.Interfaces;

public interface IVacancyDetailsClient
{
    Task<VacancyDetails?> GetAsync(
        string vacancyId,
        CancellationToken cancellationToken);
}