using System.Globalization;
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
        var hasEnabledDeviceRules = false;
        var resolvedDeviceRules = 0;
        var skippedOptionalDeviceRules = 0;

        foreach (var rule in rules.Where(r => r.IsEnabled).OrderBy(r => r.SortOrder))
        {
            var isDeviceRule = IsDeviceRule(rule);
            hasEnabledDeviceRules |= isDeviceRule;

            if (string.IsNullOrWhiteSpace(rule.TargetFieldCode))
            {
                issues.Add(new MappingIssue(
                    MappingIssueSeverity.Error,
                    "TargetFieldCode is empty.",
                    rule.SourcePath,
                    rule.TargetFieldCode));

                continue;
            }

            var template = string.IsNullOrWhiteSpace(rule.OutputTemplate) ? "{value}" : rule.OutputTemplate;
            if (string.IsNullOrWhiteSpace(rule.SourcePath))
            {
                if (string.IsNullOrWhiteSpace(rule.OutputTemplate))
                {
                    issues.Add(new MappingIssue(
                        MappingIssueSeverity.Error,
                        "SourcePath and OutputTemplate are empty.",
                        rule.SourcePath,
                        rule.TargetFieldCode));

                    continue;
                }

                records.Add(new ExportFieldRecord(
                    rule.TargetFieldCode,
                    RenderTemplate(template, string.Empty, patientData, measurementMap),
                    rule.SortOrder));
                continue;
            }

            if (!TryResolveSource(rule.SourcePath, patientData, measurementMap, out var sourceValue))
            {
                if (ShouldSkipMissingOptionalPreparedLine(rule))
                {
                    if (isDeviceRule)
                    {
                        skippedOptionalDeviceRules++;
                    }

                    continue;
                }

                issues.Add(new MappingIssue(
                    MappingIssueSeverity.Error,
                    $"Source value not found: {rule.SourcePath} -> TargetFieldCode {rule.TargetFieldCode}",
                    rule.SourcePath,
                    rule.TargetFieldCode));

                continue;
            }

            var rendered = RenderTemplate(template, sourceValue, patientData, measurementMap);

            records.Add(new ExportFieldRecord(rule.TargetFieldCode, rendered, rule.SortOrder));
            if (isDeviceRule)
            {
                resolvedDeviceRules++;
            }
        }

        if (hasEnabledDeviceRules
            && resolvedDeviceRules == 0
            && skippedOptionalDeviceRules > 0
            && !issues.Any(issue => issue.Severity == MappingIssueSeverity.Error))
        {
            issues.Add(new MappingIssue(
                MappingIssueSeverity.Error,
                "No exportable device measurements were found.",
                string.Empty,
                string.Empty));
        }

        return new MappingResult(records, issues);
    }

    private static bool ShouldSkipMissingOptionalPreparedLine(MappingRule rule)
    {
        return IsOptionalPreparedDeviceLine(rule.SourcePath);
    }

    private static bool IsDeviceRule(MappingRule rule)
    {
        return rule.SourcePath?.StartsWith("Device.", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static bool IsOptionalPreparedDeviceLine(string? sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath)
            || !sourcePath.StartsWith("Device.Measure[", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return sourcePath.EndsWith("/HeaderLine", StringComparison.OrdinalIgnoreCase)
            || sourcePath.EndsWith("/MedistarLine", StringComparison.OrdinalIgnoreCase)
            || Regex.IsMatch(sourcePath, @"/MedistarLine\d+$", RegexOptions.IgnoreCase)
            || sourcePath.Contains("/Tono/", StringComparison.OrdinalIgnoreCase)
            || sourcePath.Contains("/Pachy/", StringComparison.OrdinalIgnoreCase);
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

            if (sourceToken.Equals("Date", StringComparison.OrdinalIgnoreCase))
            {
                return DateTime.Now.ToString(string.IsNullOrWhiteSpace(format) ? "ddMMyyyy" : format, CultureInfo.InvariantCulture);
            }

            if (sourceToken.Equals("Time", StringComparison.OrdinalIgnoreCase))
            {
                return DateTime.Now.ToString(string.IsNullOrWhiteSpace(format) ? "HHmmss" : format, CultureInfo.InvariantCulture);
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
            var current when current.Equals("AIS.PatientNumber", StringComparison.OrdinalIgnoreCase) => patientData.PatientNumber ?? string.Empty,
            var current when current.Equals("AIS.LastName", StringComparison.OrdinalIgnoreCase) => patientData.LastName ?? string.Empty,
            var current when current.Equals("AIS.FirstName", StringComparison.OrdinalIgnoreCase) => patientData.FirstName ?? string.Empty,
            var current when current.Equals("AIS.BirthDate", StringComparison.OrdinalIgnoreCase)
                || current.Equals("AIS.DateOfBirth", StringComparison.OrdinalIgnoreCase) => patientData.BirthDate ?? string.Empty,
            var current when current.Equals("AIS.Street", StringComparison.OrdinalIgnoreCase) => patientData.Street ?? string.Empty,
            var current when current.Equals("AIS.PostalCodeCity", StringComparison.OrdinalIgnoreCase) => patientData.PostalCodeCity ?? string.Empty,
            var current when current.Equals("AIS.GenderCode", StringComparison.OrdinalIgnoreCase) => patientData.GenderCode ?? string.Empty,
            var current when current.Equals("AIS.SourceSystem", StringComparison.OrdinalIgnoreCase) => patientData.SourceSystem ?? string.Empty,
            var current when current.Equals("AIS.TargetSystem", StringComparison.OrdinalIgnoreCase) => patientData.TargetSystem ?? string.Empty,
            var current when current.Equals("AIS.GdtVersion", StringComparison.OrdinalIgnoreCase) => patientData.GdtVersion ?? string.Empty,
            var current when current.Equals("AIS.ExaminationType", StringComparison.OrdinalIgnoreCase)
                || current.Equals("AIS.ExamType", StringComparison.OrdinalIgnoreCase) => patientData.ExaminationType ?? string.Empty,
            _ => string.Empty
        };

        return sourcePath.StartsWith("AIS.", StringComparison.OrdinalIgnoreCase) && value != string.Empty;
    }
}
