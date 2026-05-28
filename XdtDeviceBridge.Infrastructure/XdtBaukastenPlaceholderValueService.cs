using System.Globalization;
using System.Text.RegularExpressions;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class XdtBaukastenPlaceholderValueService
{
    private const int MaximumDevicePlaceholders = 80;

    public IReadOnlyList<XdtBaukastenPlaceholder> CreateDevicePlaceholders(
        DeviceProfileDefinition? deviceProfile,
        IReadOnlyList<MeasurementValue> measurements)
    {
        if (deviceProfile is null)
        {
            return new[]
            {
                new XdtBaukastenPlaceholder("Gerät", "Kein Gerät geladen", "{Device.Value}", "Bitte zuerst ein Geräteprofil laden.", IsPreparedOnly: true)
            };
        }

        var compatibleMeasurements = IsCompatibleWithDeviceProfile(deviceProfile, measurements)
            ? measurements
            : Array.Empty<MeasurementValue>();
        var measurementValues = CreateMeasurementValueMap(compatibleMeasurements);

        var selectedDefinitions = deviceProfile.Measurements
            .Select((measurement, index) => new MeasurementDefinitionCandidate(
                measurement,
                index,
                HasValue: measurementValues.ContainsKey(measurement.SourcePath)))
            .GroupBy(
                item => string.IsNullOrWhiteSpace(item.Definition.DisplayName)
                    ? item.Definition.SourcePath
                    : item.Definition.DisplayName,
                StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(item => item.HasValue)
                .ThenByDescending(item => item.Definition.IsRequired)
                .ThenBy(item => item.Index)
                .First())
            .OrderByDescending(item => item.HasValue)
            .ThenByDescending(item => item.Definition.IsRequired)
            .ThenBy(item => item.Index)
            .Take(MaximumDevicePlaceholders)
            .Select(item => CreatePlaceholder(item.Definition, measurementValues))
            .ToList();

        if (selectedDefinitions.Count == 0)
        {
            selectedDefinitions.Add(new XdtBaukastenPlaceholder("Gerät", "Gerätewert", "{Device.Value}", "Generischer Messwert-Platzhalter für Entwürfe.", IsPreparedOnly: true));
        }

        return selectedDefinitions;
    }

    public bool IsCompatibleWithDeviceProfile(DeviceProfileDefinition? deviceProfile, IReadOnlyList<MeasurementValue> measurements)
    {
        if (deviceProfile is null || measurements.Count == 0)
        {
            return false;
        }

        var modelName = measurements.FirstOrDefault(measurement =>
            string.Equals(measurement.SourcePath, "Common/ModelName", StringComparison.OrdinalIgnoreCase))?.Value;
        if (!string.IsNullOrWhiteSpace(modelName) && !string.IsNullOrWhiteSpace(deviceProfile.Model))
        {
            return NormalizeModelName(modelName) == NormalizeModelName(deviceProfile.Model);
        }

        var profileSourcePaths = deviceProfile.Measurements
            .Where(measurement => !IsCommonMeasurement(measurement.SourcePath))
            .Select(measurement => measurement.SourcePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return measurements.Any(measurement =>
            !IsCommonMeasurement(measurement.SourcePath)
            && profileSourcePaths.Contains(measurement.SourcePath));
    }

    private static XdtBaukastenPlaceholder CreatePlaceholder(
        DeviceMeasurementDefinition measurement,
        IReadOnlyDictionary<string, string> measurementValues)
    {
        return new XdtBaukastenPlaceholder(
            "Gerät",
            string.IsNullOrWhiteSpace(measurement.DisplayName) ? measurement.SourcePath : measurement.DisplayName,
            "{Device." + measurement.SourcePath + "}",
            $"{measurement.SourcePath} ({measurement.Unit ?? "ohne Einheit"})",
            measurementValues.TryGetValue(measurement.SourcePath, out var value) ? DisplayPlaceholderValue(FormatMeasurementValue(measurement, value)) : "-");
    }

    private static Dictionary<string, string> CreateMeasurementValueMap(IReadOnlyList<MeasurementValue> measurements)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var measurement in measurements)
        {
            if (!string.IsNullOrWhiteSpace(measurement.SourcePath) && !string.IsNullOrWhiteSpace(measurement.Value))
            {
                values.TryAdd(measurement.SourcePath, measurement.Value);
            }
        }

        return values;
    }

    private static bool IsCommonMeasurement(string sourcePath)
    {
        return sourcePath.StartsWith("Common/", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeModelName(string value)
    {
        return Regex.Replace(value, "[^A-Za-z0-9]", string.Empty).ToUpperInvariant();
    }

    private static string DisplayPlaceholderValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "-";
        }

        var trimmed = value.Trim();
        return trimmed.Length <= 32 ? trimmed : trimmed[..29] + "...";
    }

    private static string FormatMeasurementValue(DeviceMeasurementDefinition measurement, string value)
    {
        var trimmed = value.Trim();
        if (!string.Equals(measurement.Unit, "dpt", StringComparison.OrdinalIgnoreCase)
            || !decimal.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var numericValue)
            || numericValue <= 0
            || trimmed.StartsWith('+'))
        {
            return trimmed;
        }

        return "+" + trimmed;
    }

    private sealed record MeasurementDefinitionCandidate(
        DeviceMeasurementDefinition Definition,
        int Index,
        bool HasValue);
}
