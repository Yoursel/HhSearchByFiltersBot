using System.Text.Json;
using HhBot.Application.Interfaces;
using HhBot.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HhBot.Infrastructure.Hh.Persistence;

public sealed class JsonSentVacancyStore(
    IOptions<PersistenceOptions> options,
    ILogger<JsonSentVacancyStore> logger) : ISentVacancyStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly PersistenceOptions _options = options.Value;
    private readonly SemaphoreSlim _lock = new(1, 1);
    
    public async Task<IReadOnlySet<string>> GetSentVacancyIdsAsync(
        IReadOnlyCollection<string> vacancyIds,
        CancellationToken cancellationToken)
    {
        if (vacancyIds.Count == 0)
            return new HashSet<string>();

        await _lock.WaitAsync(cancellationToken);

        try
        {
            var records = await ReadRecordsAsync(cancellationToken);
            var knownIds = records.Select(record => record.VacancyId).ToHashSet();

            return vacancyIds
                .Where(knownIds.Contains)
                .ToHashSet();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task MarkAsSentAsync(Domain.Vacancy vacancy, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var records = await ReadRecordsAsync(cancellationToken);

            if (records.Any(record => record.VacancyId == vacancy.Id))
                return;

            records.Add(new SentVacancyRecord(
                vacancy.Id,
                vacancy.Title,
                vacancy.EmployerName,
                vacancy.Url,
                DateTimeOffset.Now));

            await WriteRecordsAsync(records, cancellationToken);

            logger.LogInformation("Vacancy marked as sent: {VacancyId}", vacancy.Id);
        }
        finally
        {
            _lock.Release();
        }
    }
    
    private async Task<List<SentVacancyRecord>> ReadRecordsAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_options.SentVacanciesFilePath))
            return [];

        await using var stream = File.OpenRead(_options.SentVacanciesFilePath);

        var records = await JsonSerializer.DeserializeAsync<List<SentVacancyRecord>>(
            stream,
            JsonOptions,
            cancellationToken);

        return records ?? [];
    }
    
    private async Task WriteRecordsAsync(
        List<SentVacancyRecord> records,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_options.SentVacanciesFilePath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await using var stream = File.Create(_options.SentVacanciesFilePath);

        await JsonSerializer.SerializeAsync(
            stream,
            records,
            JsonOptions,
            cancellationToken);
    }
}