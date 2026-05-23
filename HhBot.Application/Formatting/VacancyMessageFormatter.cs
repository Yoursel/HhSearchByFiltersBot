using System.Net;
using System.Text.RegularExpressions;
using HhBot.Domain;

namespace HhBot.Application.Formatting;

public sealed partial class VacancyMessageFormatter
{
    private const int MaxDescriptionLength = 1200;

    public string Format(Vacancy vacancy, VacancyDetails? details)
    {
        var employer = string.IsNullOrWhiteSpace(vacancy.EmployerName)
            ? "Компания не указана"
            : vacancy.EmployerName;

        var publishedAt = vacancy.PublishedAt?.ToString("dd.MM.yyyy HH:mm");

        var description = NormalizeDescription(details?.Description);

        var message = publishedAt is null
            ? $"{vacancy.Title}\n{employer}\n\n{description}\n\n{vacancy.Url}"
            : $"{vacancy.Title}\n{employer}\nОпубликовано: {publishedAt}\n\n{description}\n\n{vacancy.Url}";

        return message.Trim();
    }

    private static string NormalizeDescription(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return "Описание не указано.";

        var text = html
            .Replace("<br>", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("<br/>", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("<br />", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("</p>", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("</li>", "\n", StringComparison.OrdinalIgnoreCase);

        text = HtmlTagRegex().Replace(text, string.Empty);
        text = WebUtility.HtmlDecode(text);
        text = BlankLinesRegex().Replace(text, "\n\n").Trim();

        if (text.Length <= MaxDescriptionLength)
            return text;

        return text[..MaxDescriptionLength].TrimEnd() + "...";
    }

    [GeneratedRegex("<.*?>", RegexOptions.Singleline)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex BlankLinesRegex();
}