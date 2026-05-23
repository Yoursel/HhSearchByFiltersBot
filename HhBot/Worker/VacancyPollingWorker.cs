using HhBot.Application.Interfaces;
using HhBot.Application.Options;
using HhBot.Application.UseCases;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HhBot.Worker;

public sealed class VacancyPollingWorker(
    ILogger<VacancyPollingWorker> logger,
    IOptions<SearchOptions> searchOptions,
    INotificationSender notificationSender,
    CheckVacanciesUseCase checkVacanciesUseCase) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = searchOptions.Value;
        var interval = TimeSpan.FromMinutes(options.CheckIntervalMinutes);

        logger.LogInformation("HhBot started. Check interval: {IntervalMinutes} minutes", options.CheckIntervalMinutes);

        try
        {
            await notificationSender.SendAsync("HhBot started successfully", stoppingToken);
            logger.LogInformation("Startup Telegram notification sent");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Startup Telegram notification failed");
        }
        
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Vacancy check started. Published from: {PublishedFrom}", options.PublishedFrom);

            try
            {
                await checkVacanciesUseCase.ExecuteAsync(options, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Vacancy check failed");
            }

            logger.LogInformation("Vacancy check finished. Next check in {IntervalMinutes} minutes", options.CheckIntervalMinutes);

            await Task.Delay(interval, stoppingToken);
        }
    }
}