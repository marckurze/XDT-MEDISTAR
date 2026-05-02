using System.Xml.Linq;

namespace XdtDeviceBridge.Core;

public sealed class XmlDeviceParser
{
    private static readonly HashSet<string> KnownGroups = new(StringComparer.OrdinalIgnoreCase)
    {
        "ARMedian", "ARList", "TrialLens", "ContactLens", "PDList"
    };

    public DeviceParseResult ParseFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must not be empty.", nameof(path));
        }

        var measurements = new List<MeasurementValue>();
        var issues = new List<DeviceParseIssue>();

        try
        {
            var document = XDocument.Load(path, LoadOptions.PreserveWhitespace);
            if (document.Root is null)
            {
                issues.Add(new DeviceParseIssue(DeviceParseIssueSeverity.Error, "XML document has no root element.", "/", null));
                return new DeviceParseResult(measurements, issues);
            }

            foreach (var child in document.Root.Elements())
            {
                ParseElement(child, string.Empty, measurements);
            }
        }
        catch (Exception ex) when (ex is System.Xml.XmlException or IOException or UnauthorizedAccessException)
        {
            issues.Add(new DeviceParseIssue(DeviceParseIssueSeverity.Error, $"Failed to parse XML: {ex.Message}", "/", null));
        }

        return new DeviceParseResult(measurements, issues);
    }

    private static void ParseElement(XElement element, string parentPath, List<MeasurementValue> measurements)
    {
        var currentSegment = BuildSegment(element);
        var currentPath = string.IsNullOrEmpty(parentPath) ? currentSegment : $"{parentPath}/{currentSegment}";

        if (!element.HasElements)
        {
            var value = (element.Value ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(value))
            {
                measurements.Add(new MeasurementValue(
                    SourcePath: currentPath,
                    DisplayName: element.Name.LocalName,
                    Value: value,
                    Unit: element.Attribute("Unit")?.Value,
                    Eye: DetectEye(currentPath),
                    Group: DetectGroup(currentPath)));
            }

            return;
        }

        foreach (var child in element.Elements())
        {
            ParseElement(child, currentPath, measurements);
        }
    }

    private static string BuildSegment(XElement element)
    {
        var noAttribute = element.Attribute("No");
        if (noAttribute is not null)
        {
            return $"{element.Name.LocalName}[@No='{noAttribute.Value}']";
        }

        return element.Name.LocalName;
    }

    private static string? DetectEye(string path)
    {
        if (path.StartsWith("R/", StringComparison.Ordinal))
        {
            return "R";
        }

        if (path.StartsWith("L/", StringComparison.Ordinal))
        {
            return "L";
        }

        return null;
    }

    private static string? DetectGroup(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            var normalized = segment.Split('[')[0];
            if (KnownGroups.Contains(normalized))
            {
                return normalized;
            }
        }

        return null;
    }
}
