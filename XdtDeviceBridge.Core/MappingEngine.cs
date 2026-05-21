using System.Text.RegularExpressions;

namespace XdtDeviceBridge.Core;

public sealed class MappingEngine
{
    private static readonly Regex PlaceholderRegex = new("\\{([^{}]+)\\}", RegexOptions.Compiled);
    private static readonly MedistarResultFormatter MedistarFormatter = new();

    public MappingResult Map(PatientData patientData, IEnumerable<MeasurementValue> measurements, IEnumerable<MappingRule> rules)
    {
        var issues = new List<MappingIssue>();
        var records = new List<ExportFieldRecord>();
        var measurementMap = CreateMeasurementMap(measurements);

        foreach (var rule in rules.Where(r => r.IsEnabled).OrderBy(r => r.SortOrder))
        {
            if (string.IsNullOrWhiteSpace(rule.TargetFieldCode))
            {
                issues.Add(new MappingIssue(
                    MappingIssueSeverity.Error,
                    "TargetFieldCode is empty.",
                    rule.SourcePath,
                    rule.TargetFieldCode));

                continue;
            }

            if (!TryResolveSource(rule.SourcePath, patientData, measurementMap, out var sourceValue))
            {
                if (ShouldSkipMissingOptionalPreparedLine(rule))
                {
                    continue;
                }

                issues.Add(new MappingIssue(
                    MappingIssueSeverity.Error,
                    $"Source value not found: {rule.SourcePath} -> TargetFieldCode {rule.TargetFieldCode}",
                    rule.SourcePath,
                    rule.TargetFieldCode));

                continue;
            }

            var template = string.IsNullOrWhiteSpace(rule.OutputTemplate) ? "{value}" : rule.OutputTemplate;
            var rendered = RenderTemplate(template, sourceValue, patientData, measurementMap);

            records.Add(new ExportFieldRecord(rule.TargetFieldCode, rendered, rule.SortOrder));
        }

        return new MappingResult(records, issues);
    }

    private static bool ShouldSkipMissingOptionalPreparedLine(MappingRule rule)
    {
        return string.Equals(rule.TargetFieldCode, "6227", StringComparison.Ordinal)
            && rule.SourcePath.StartsWith("Device.Measure[@Type='SBJ']/MedistarLine", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryResolveSource(
        string sourcePath,
        PatientData patientData,
        Dictionary<string, string> measurements,
        out string value)
    {
        if (TryResolvePatient(sourcePath, patientData, out value))
        {
            return true;
        }

        if (sourcePath.StartsWith("Device.", StringComparison.OrdinalIgnoreCase))
        {
            var key = sourcePath[7..];
            if (measurements.TryGetValue(key, out var measurementValue))
            {
                value = measurementValue;
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    private static string RenderTemplate(
        string template,
        string sourceValue,
        PatientData patientData,
        Dictionary<string, string> measurements)
    {
        return PlaceholderRegex.Replace(template, match =>
        {
            var token = match.Groups[1].Value;
            var (sourceToken, format) = SplitFormatToken(token);

            if (string.Equals(sourceToken, "value", StringComparison.OrdinalIgnoreCase))
            {
                return ApplyFormat(sourceValue, format);
            }

            if (sourceToken.StartsWith("patient.", StringComparison.OrdinalIgnoreCase)
                && TryResolvePatient($"AIS.{sourceToken[8..]}", patientData, out var patientValue))
            {
                return ApplyFormat(patientValue, format);
            }

            if (sourceToken.StartsWith("AIS.", StringComparison.OrdinalIgnoreCase)
                && TryResolvePatient(sourceToken, patientData, out var aisValue))
            {
                return ApplyFormat(aisValue, format);
            }

            if (sourceToken.StartsWith("Device.", StringComparison.OrdinalIgnoreCase)
                && measurements.TryGetValue(sourceToken[7..], out var measurementValue))
            {
                return ApplyFormat(measurementValue, format);
            }

            return string.Empty;
        });
    }

    private static Dictionary<string, string> CreateMeasurementMap(IEnumerable<MeasurementValue> measurements)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var measurement in measurements)
        {
            if (string.IsNullOrWhiteSpace(measurement.SourcePath))
            {
                continue;
            }

            map.TryAdd(measurement.SourcePath, measurement.Value);
        }

        return map;
    }

    private static (string SourceToken, string? Format) SplitFormatToken(string token)
    {
        var separatorIndex = token.LastIndexOf(':');
        if (separatorIndex < 0)
        {
            return (token, null);
        }

        return (token[..separatorIndex], token[(separatorIndex + 1)..]);
    }

    private static string ApplyFormat(string? value, string? format)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return value ?? string.Empty;
        }

        return format.Trim() switch
        {
            var currentFormat when currentFormat.Equals("Raw", StringComparison.OrdinalIgnoreCase)
                => MedistarFormatter.FormatRaw(value),
            var currentFormat when currentFormat.Equals("Diopter", StringComparison.OrdinalIgnoreCase)
                => MedistarFormatter.FormatDiopter(value),
            var currentFormat when currentFormat.Equals("Axis", StringComparison.OrdinalIgnoreCase)
                => MedistarFormatter.FormatAxis(value),
            var currentFormat when currentFormat.Equals("Pd", StringComparison.OrdinalIgnoreCase)
                => MedistarFormatter.FormatPd(value),
            var currentFormat when currentFormat.Equals("Iop", StringComparison.OrdinalIgnoreCase)
                => MedistarFormatter.FormatIop(value),
            var currentFormat when currentFormat.Equals("Pachy", StringComparison.OrdinalIgnoreCase)
                => MedistarFormatter.FormatPachy(value),
            var currentFormat when currentFormat.Equals("Prism", StringComparison.OrdinalIgnoreCase)
                => MedistarFormatter.FormatPrism(value),
            var currentFormat when currentFormat.Equals("Keratometry", StringComparison.OrdinalIgnoreCase)
                => MedistarFormatter.FormatKeratometry(value),
            var currentFormat when currentFormat.Equals("Time", StringComparison.OrdinalIgnoreCase)
                => MedistarFormatter.FormatTime(value),
            _ => value ?? string.Empty
        };
    }

    private static bool TryResolvePatient(string sourcePath, PatientData patientData, out string value)
    {
        value = sourcePath switch
        {
            "AIS.PatientNumber" => patientData.PatientNumber ?? string.Empty,
            "AIS.LastName" => patientData.LastName ?? string.Empty,
            "AIS.FirstName" => patientData.FirstName ?? string.Empty,
            "AIS.BirthDate" => patientData.BirthDate ?? string.Empty,
            "AIS.Street" => patientData.Street ?? string.Empty,
            "AIS.PostalCodeCity" => patientData.PostalCodeCity ?? string.Empty,
            "AIS.GenderCode" => patientData.GenderCode ?? string.Empty,
            "AIS.SourceSystem" => patientData.SourceSystem ?? string.Empty,
            "AIS.TargetSystem" => patientData.TargetSystem ?? string.Empty,
            "AIS.GdtVersion" => patientData.GdtVersion ?? string.Empty,
            "AIS.ExaminationType" => patientData.ExaminationType ?? string.Empty,
            _ => string.Empty
        };

        return sourcePath.StartsWith("AIS.", StringComparison.OrdinalIgnoreCase) && value != string.Empty;
    }
}
