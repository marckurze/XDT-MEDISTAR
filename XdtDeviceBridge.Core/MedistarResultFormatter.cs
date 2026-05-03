namespace XdtDeviceBridge.Core;

public sealed class MedistarResultFormatter
{
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
        return value?.Trim() ?? string.Empty;
    }
}
