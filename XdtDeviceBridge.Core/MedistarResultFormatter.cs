namespace XdtDeviceBridge.Core;

public sealed class MedistarResultFormatter
{
    public string FormatRaw(string? value)
    {
        return TrimOrEmpty(value);
    }

    public string FormatDiopter(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return string.Empty;
        }

        if (trimmed[0] is '+' or '-')
        {
            return $"{trimmed[0]} {trimmed[1..].TrimStart()}";
        }

        if (char.IsDigit(trimmed[0]) || trimmed[0] == '.')
        {
            return $"+ {trimmed}";
        }

        return trimmed;
    }

    public string FormatAxis(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return string.Empty;
        }

        return trimmed.PadLeft(3);
    }

    public string FormatPd(string? value)
    {
        return TrimOrEmpty(value);
    }

    public string FormatIop(string? value)
    {
        return TrimOrEmpty(value);
    }

    public string FormatPachy(string? value)
    {
        return TrimOrEmpty(value);
    }

    public string FormatPrism(string? value)
    {
        return TrimOrEmpty(value);
    }

    public string FormatKeratometry(string? value)
    {
        return TrimOrEmpty(value);
    }

    public string FormatTime(string? value)
    {
        var trimmed = TrimOrEmpty(value);
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        if (IsClockTime(trimmed))
        {
            return trimmed;
        }

        if (trimmed.Length is 4 or 6 && trimmed.All(char.IsDigit))
        {
            var hourText = trimmed[..2];
            var minuteText = trimmed.Substring(2, 2);
            if (int.TryParse(hourText, out var hour)
                && int.TryParse(minuteText, out var minute)
                && hour is >= 0 and <= 23
                && minute is >= 0 and <= 59)
            {
                return $"{hourText}:{minuteText}";
            }
        }

        return trimmed;
    }

    private static bool IsClockTime(string value)
    {
        return value.Length == 5
            && value[2] == ':'
            && char.IsDigit(value[0])
            && char.IsDigit(value[1])
            && char.IsDigit(value[3])
            && char.IsDigit(value[4]);
    }

    private static string TrimOrEmpty(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
