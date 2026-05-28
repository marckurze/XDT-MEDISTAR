using System.Globalization;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class XdtBaukastenPlaceholderValueService
{
    private const int MaximumDevicePlaceholders = 80;
    private readonly XdtBaukastenDeviceCompatibilityService _compatibilityService;

    public XdtBaukastenPlaceholderValueService()
        : this(new XdtBaukastenDeviceCompatibilityService())
    {
    }

    public XdtBaukastenPlaceholderValueService(XdtBaukastenDeviceCompatibilityService compatibilityService)
    {
        _compatibilityService = compatibilityService ?? throw new ArgumentNullException(nameof(compatibilityService));
    }

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

        var compatibleMeasurements = _compatibilityService.IsCompatibleWithDeviceProfile(deviceProfile, measurements)
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

        if (!deviceProfile.Metadata.IsBuiltIn
            && compatibleMeasurements.Count > 0
            && (selectedDefinitions.Count == 0 || selectedDefinitions.All(placeholder => placeholder.ExampleValue == "-")))
        {
            selectedDefinitions = CreateDynamicParsedPlaceholders(compatibleMeasurements).ToList();
        }

        if (selectedDefinitions.Count == 0)
        {
            selectedDefinitions.Add(new XdtBaukastenPlaceholder("Gerät", "Gerätewert", "{Device.Value}", "Generischer Messwert-Platzhalter für Entwürfe.", IsPreparedOnly: true));
        }

        return selectedDefinitions;
    }

    public bool IsCompatibleWithDeviceProfile(DeviceProfileDefinition? deviceProfile, IReadOnlyList<MeasurementValue> measurements)
    {
        return _compatibilityService.IsCompatibleWithDeviceProfile(deviceProfile, measurements);
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

    private static IEnumerable<XdtBaukastenPlaceholder> CreateDynamicParsedPlaceholders(IReadOnlyList<MeasurementValue> measurements)
    {
        return measurements
            .Where(measurement => !IsCommonParsedMeasurement(measurement.SourcePath))
            .Where(measurement => !IsAttributeMeasurement(measurement.SourcePath))
            .Where(measurement => !string.IsNullOrWhiteSpace(measurement.Value))
            .GroupBy(measurement => measurement.SourcePath, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(measurement => GetDynamicPlaceholderSortRank(measurement.SourcePath))
            .ThenBy(measurement => measurement.SourcePath, StringComparer.OrdinalIgnoreCase)
            .Take(MaximumDevicePlaceholders)
            .Select(measurement => new XdtBaukastenPlaceholder(
                "Gerät",
                string.IsNullOrWhiteSpace(measurement.DisplayName) ? measurement.SourcePath : measurement.DisplayName,
                "{Device." + measurement.SourcePath + "}",
                $"{measurement.SourcePath} ({measurement.Unit ?? "ohne Einheit"})",
                DisplayPlaceholderValue(FormatParsedMeasurementValue(measurement))));
    }

    private static bool IsCommonParsedMeasurement(string sourcePath)
    {
        return sourcePath.StartsWith("Common/", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourcePath, "Company", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourcePath, "ModelName", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourcePath, "MachineNo", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourcePath, "ROMVersion", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourcePath, "Version", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourcePath, "Date", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourcePath, "Time", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAttributeMeasurement(string sourcePath)
    {
        return sourcePath.Contains("/@", StringComparison.Ordinal);
    }

    private static int GetDynamicPlaceholderSortRank(string sourcePath)
    {
        if (sourcePath.Contains("/MedistarLine", StringComparison.OrdinalIgnoreCase)
            || sourcePath.EndsWith("/Sphere", StringComparison.OrdinalIgnoreCase)
            || sourcePath.EndsWith("/Sphare", StringComparison.OrdinalIgnoreCase)
            || sourcePath.EndsWith("/Cylinder", StringComparison.OrdinalIgnoreCase)
            || sourcePath.EndsWith("/Axis", StringComparison.OrdinalIgnoreCase)
            || sourcePath.EndsWith("/ADD", StringComparison.OrdinalIgnoreCase)
            || sourcePath.EndsWith("/PD", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (sourcePath.Contains("/R/", StringComparison.OrdinalIgnoreCase)
            || sourcePath.Contains("/L/", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return 2;
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
        if (!IsDiopterUnit(measurement.Unit)
            || !decimal.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var numericValue)
            || numericValue <= 0
            || trimmed.StartsWith('+'))
        {
            return trimmed;
        }

        return "+" + trimmed;
    }

    private static string FormatParsedMeasurementValue(MeasurementValue measurement)
    {
        var trimmed = measurement.Value.Trim();
        if (!IsDiopterUnit(measurement.Unit)
            || !decimal.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var numericValue)
            || numericValue <= 0
            || trimmed.StartsWith('+'))
        {
            return trimmed;
        }

        return "+" + trimmed;
    }

    private static bool IsDiopterUnit(string? unit)
    {
        return string.Equals(unit, "dpt", StringComparison.OrdinalIgnoreCase)
            || string.Equals(unit, "D", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record MeasurementDefinitionCandidate(
        DeviceMeasurementDefinition Definition,
        int Index,
        bool HasValue);
}
