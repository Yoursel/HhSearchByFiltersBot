using System.Net.Http.Headers;
using System.Net.Http.Json;
using HhBot.Application.Interfaces;
using HhBot.Domain;
using HhBot.Infrastructure.Hh.Dto;
using HhBot.Infrastructure.Hh.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HhBot.Infrastructure.Hh;

public sealed class HhVacancyDetailsClient(
    HttpClient httpClient,
    IOptions<HhApiOptions> options,
    ILogger<HhVacancyDetailsClient> logger) : IVacancyDetailsClient
{
    private readonly HhApiOptions _options = options.Value;
    
    public async Task<VacancyDetails?> GetAsync(string vacancyId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vacancyId);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"vacancies/{vacancyId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        request.Headers.UserAgent.ParseAdd(_options.UserAgent);
        request.Headers.Add("HH-User-Agent", _options.UserAgent);

        logger.LogInformation("HH vacancy details request: {VacancyId}", vacancyId);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);

            logger.LogError(
                "HH vacancy details request failed. VacancyId: {VacancyId}. StatusCode: {StatusCode}. Body: {Body}",
                vacancyId,
                response.StatusCode,
                errorBody);

            response.EnsureSuccessStatusCode();
        }

        var dto = await response.Content.ReadFromJsonAsync<HhVacancyDetailsDto>(
            cancellationToken);

        if (dto?.Id is null)
        {
            logger.LogWarning("HH vacancy details response is empty. VacancyId: {VacancyId}", vacancyId);
            return null;
        }

        return new VacancyDetails(
            dto.Id,
            dto.Description);
    }
}