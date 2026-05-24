using System.Net.Http.Headers;
using System.Net.Http.Json;
using HhBot.Application.Interfaces;
using HhBot.Application.Options;
using HhBot.Infrastructure.Hh.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HhBot.Infrastructure.Hh;

public class HhVacancyClient(HttpClient httpClient,
    IOptions<HhApiOptions> options,
    ILogger<HhVacancyClient> logger) : IVacancySearchClient
{
    private readonly HhApiOptions _options = options.Value;
    
    public async Task<IReadOnlyList<Domain.Vacancy>> SearchAsync(SearchOptions searchOptions, CancellationToken cancellationToken)
    {
        var searchTexts = searchOptions.Keywords.Length > 0
            ? searchOptions.Keywords
            : searchOptions.SkillKeywords;

        var vacanciesById = new Dictionary<string, Domain.Vacancy>();

        foreach (var searchText in searchTexts
                     .Where(text => !string.IsNullOrWhiteSpace(text))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var vacancies = await SearchSingleTextAsync(searchText, searchOptions, cancellationToken);

            foreach (var vacancy in vacancies)
            {
                vacanciesById.TryAdd(vacancy.Id, vacancy);
            }
        }

        return vacanciesById.Values.ToList();
    }

    private async Task<IReadOnlyList<Domain.Vacancy>> SearchSingleTextAsync(
        string searchText,
        SearchOptions searchOptions,
        CancellationToken cancellationToken)
    {
        var parameters = new List<KeyValuePair<string, string>>
        {
            new("text", searchText),
            new("date_from", searchOptions.PublishedFrom.ToString("yyyy-MM-dd")),
            new("order_by", "publication_time"),
            new("per_page", searchOptions.MaxVacanciesPerRun.ToString())
        };
        
        var workFormat = searchOptions.WorkMode.ToLowerInvariant() switch
        {
            "remote" => "REMOTE",
            "office" => "ON_SITE",
            "hybrid" => "HYBRID",
            _ => null
        };

        if (workFormat is not null)
            parameters.Add(new("work_format", workFormat));
        
        parameters.AddRange(searchOptions.ExperienceIds.Select(experience => new KeyValuePair<string, string>("experience", experience)));
        parameters.AddRange(searchOptions.AreaIds.Select(area => new KeyValuePair<string, string>("area", area)));

        using var queryContent = new FormUrlEncodedContent(parameters);
        var query = await queryContent.ReadAsStringAsync(cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"vacancies?{query}");
        
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.AccessToken);
        request.Headers.UserAgent.ParseAdd(_options.UserAgent);
        request.Headers.Add("HH-User-Agent", _options.UserAgent);
        
        logger.LogInformation("HH vacancy search request: {RequestUri}", request.RequestUri);
        
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);

            logger.LogError(
                "HH API request failed. StatusCode: {StatusCode}. Body: {Body}",
                response.StatusCode,
                errorBody);

            response.EnsureSuccessStatusCode();
        }
        
        var searchResponse = await response.Content.ReadFromJsonAsync<HhVacancySearchResponse>(
            cancellationToken);
        
        if (searchResponse is null)
        {
            logger.LogWarning("HH API returned empty response");
            return [];
        }
        
        return searchResponse.Items
            .Where(item =>
                !string.IsNullOrWhiteSpace(item.Id) &&
                !string.IsNullOrWhiteSpace(item.Name) &&
                !string.IsNullOrWhiteSpace(item.AlternateUrl))
            .Select(item => new Domain.Vacancy(
                item.Id!,
                item.Name!,
                item.Employer?.Name,
                item.AlternateUrl!,
                DateTimeParser.ParseNullable(item.PublishedAt)))
            .ToList();
    }
}
