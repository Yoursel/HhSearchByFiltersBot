using HhBot.Application.Formatting;
using HhBot.Application.Interfaces;
using HhBot.Application.Matching;
using HhBot.Application.Options;
using Microsoft.Extensions.Logging;

namespace HhBot.Application.UseCases;

public sealed class CheckVacanciesUseCase(
    IVacancySearchClient vacancySearchClient,
    ILogger<CheckVacanciesUseCase> logger,
    ISentVacancyStore sentVacancyStore,
    INotificationSender notificationSender,
    VacancyMessageFormatter messageFormatter,
    IVacancyDetailsClient vacancyDetailsClient,
    VacancyMatcher vacancyMatcher)
{
    public async Task ExecuteAsync(SearchOptions options, CancellationToken cancellationToken)
    {
        var vacancies = await vacancySearchClient.SearchAsync(options, cancellationToken);

        var vacancyIds = vacancies.Select(v => v.Id).ToArray();
        var sentIds = await sentVacancyStore.GetSentVacancyIdsAsync(vacancyIds, cancellationToken);

        var newVacancies = vacancies
            .Where(v => !sentIds.Contains(v.Id))
            .ToArray();

        logger.LogInformation("HH vacancies received: {Count}", vacancies.Count);
        logger.LogInformation("Already sent vacancies skipped: {Count}", sentIds.Count);
        logger.LogInformation("New vacancies found: {Count}", newVacancies.Length);

        foreach (var vacancy in newVacancies)
        {
            logger.LogInformation(
                "New vacancy found: {VacancyId} | {Title} | {Employer} | {Url}",
                vacancy.Id,
                vacancy.Title,
                vacancy.EmployerName,
                vacancy.Url);

            var details = await vacancyDetailsClient.GetAsync(vacancy.Id, cancellationToken);
            
            if (!vacancyMatcher.IsMatch(vacancy, details, options, out var rejectReason))
            {
                logger.LogInformation(
                    "Vacancy rejected: {VacancyId} | {Title}. Reason: {Reason}",
                    vacancy.Id,
                    vacancy.Title,
                    rejectReason);

                await sentVacancyStore.MarkAsSentAsync(vacancy, cancellationToken);
                continue;
            }
            
            var message = messageFormatter.Format(vacancy, details);

            await notificationSender.SendAsync(message, cancellationToken);

            await sentVacancyStore.MarkAsSentAsync(vacancy, cancellationToken);
        }
    }
}