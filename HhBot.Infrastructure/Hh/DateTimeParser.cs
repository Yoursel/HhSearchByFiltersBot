namespace HhBot.Infrastructure.Hh;

internal static class DateTimeParser
{
    public static DateTimeOffset? ParseNullable(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (value.Length >= 5)
        {
            var offsetStart = value.Length - 5;
            var offset = value[offsetStart..];

            if ((offset[0] == '+' || offset[0] == '-') && offset[3] != ':')
            {
                value = value[..offsetStart] + offset[..3] + ":" + offset[3..];
            }
        }

        return DateTimeOffset.TryParse(value, out var parsed)
            ? parsed
            : null;
    }
}