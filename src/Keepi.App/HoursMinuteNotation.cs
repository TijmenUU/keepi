using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Keepi.App;

internal static partial class HoursMinuteNotation
{
    public static int ParseOrFallback(string input, int fallback)
    {
        if (!TryParse(input: input, out var result))
        {
            return fallback;
        }

        return result;
    }

    public static bool TryParse(string input, out int result)
    {
        if (!IsValid(input))
        {
            result = 0;
            return false;
        }

        var parts = input.Split(
            'u',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        if (parts.Length < 1)
        {
            result = 0;
            return true;
        }
        else if (parts.Length == 1)
        {
            if (parts[0].EndsWith('m'))
            {
                result = int.Parse(parts[0][..^1]);
                return true;
            }
            result = int.Parse(parts[0]) * 60;
            return true;
        }
        else if (parts.Length == 2)
        {
            var hoursAsMinutes = int.Parse(parts[0]) * 60;
            var minutes = int.Parse(parts[1][..^1]);

            if (hoursAsMinutes < 0)
            {
                result = hoursAsMinutes - minutes;
                return true;
            }
            result = hoursAsMinutes + minutes;
            return true;
        }
        else
        {
            throw new InvalidOperationException(
                "No more than two parts should be possible due to the regex check"
            );
        }
    }

    public static bool IsValid(string notation)
    {
        return HoursMinuteNotationRegex().IsMatch(input: notation);
    }

    [GeneratedRegex("^-?([0-9]+u)?([1-5]?[0-9]m)?$")]
    private static partial Regex HoursMinuteNotationRegex();

    public static string Format(int minutes)
    {
        var sb = new StringBuilder();

        var hours = minutes / 60;
        if (hours != 0)
        {
            sb.Append(hours);
            sb.Append('u');
        }

        var remainingMinutes = minutes - (hours * 60);
        if (remainingMinutes != 0)
        {
            if (hours < 0)
            {
                sb.Append(Math.Abs(remainingMinutes));
            }
            else
            {
                sb.Append(remainingMinutes);
            }
            sb.Append('m');
        }

        return sb.ToString();
    }
}
