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

            if (ContainsKeyword(text, keyword))
            {
                rejectReason = $"Matched exclude keyword: {keyword}";
                return false;
            }
        }

        if (options.IncludeKeywords.Length > 0 && !HasAnyKeyword(text, options.IncludeKeywords))
        {
            rejectReason = "No required include keyword matched";
            return false;
        }

        rejectReason = null;
        return true;
    }

    private static bool HasAnyKeyword(string text, IEnumerable<string> keywords)
    {
        return keywords.Any(keyword =>
            !string.IsNullOrWhiteSpace(keyword) &&
            ContainsKeyword(text, keyword));
    }

    private static bool ContainsKeyword(string text, string keyword)
    {
        var trimmedKeyword = keyword.Trim();
        var startIndex = 0;

        while (startIndex < text.Length)
        {
            var matchIndex = text.IndexOf(
                trimmedKeyword,
                startIndex,
                StringComparison.OrdinalIgnoreCase);

            if (matchIndex < 0)
                return false;

            var beforeIndex = matchIndex - 1;
            var afterIndex = matchIndex + trimmedKeyword.Length;

            if (IsKeywordBoundary(text, beforeIndex) &&
                IsKeywordBoundary(text, afterIndex))
            {
                return true;
            }

            startIndex = matchIndex + trimmedKeyword.Length;
        }

        return false;
    }

    private static bool IsKeywordBoundary(string text, int index)
    {
        if (index < 0 || index >= text.Length)
            return true;

        var character = text[index];

        return !char.IsLetterOrDigit(character) && character != '#';
    }
}
