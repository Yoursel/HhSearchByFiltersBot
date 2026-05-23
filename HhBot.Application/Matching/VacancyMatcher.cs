using HhBot.Application.Options;
using HhBot.Domain;

namespace HhBot.Application.Matching;

public sealed class VacancyMatcher
{
    public bool IsMatch(
        Vacancy vacancy,
        VacancyDetails? details,
        SearchOptions options,
        out string? rejectReason)
    {
        var text = string.Join(
            ' ',
            vacancy.Title,
            vacancy.EmployerName,
            details?.Description);

        foreach (var keyword in options.ExcludeKeywords)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                continue;

            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                rejectReason = $"Matched exclude keyword: {keyword}";
                return false;
            }
        }

        rejectReason = null;
        return true;
    }
}