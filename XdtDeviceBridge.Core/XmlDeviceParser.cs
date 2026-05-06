using System.Xml.Linq;

namespace XdtDeviceBridge.Core;

public sealed class XmlDeviceParser
{
    private static readonly HashSet<string> KnownGroups = new(StringComparer.OrdinalIgnoreCase)
    {
        "ARMedian", "ARList", "TrialLens", "ContactLens", "PDList", "LM", "PD"
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

        AddAttributeMeasurements(element, currentPath, measurements);

        if (!element.HasElements)
        {
            var value = (element.Value ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(value))
            {
                measurements.Add(new MeasurementValue(
                    SourcePath: currentPath,
                    DisplayName: element.Name.LocalName,
                    Value: value,
                    Unit: GetAttributeValue(element, "Unit"),
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
        var noAttribute = GetAttribute(element, "No");
        if (noAttribute is not null)
        {
            return $"{element.Name.LocalName}[@{noAttribute.Name.LocalName}='{noAttribute.Value}']";
        }

        var typeAttribute = GetAttribute(element, "Type");
        if (typeAttribute is not null)
        {
            return $"{element.Name.LocalName}[@{typeAttribute.Name.LocalName}='{typeAttribute.Value}']";
        }

        return element.Name.LocalName;
    }

    private static string? DetectEye(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => segment.Split('[')[0]);

        if (segments.Any(segment => string.Equals(segment, "R", StringComparison.OrdinalIgnoreCase)))
        {
            return "R";
        }

        if (segments.Any(segment => string.Equals(segment, "L", StringComparison.OrdinalIgnoreCase)))
        {
            return "L";
        }

        return null;
    }

    private static string? DetectGroup(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string? detectedGroup = null;
        foreach (var segment in segments)
        {
            var normalized = segment.Split('[')[0];
            if (KnownGroups.Contains(normalized))
            {
                detectedGroup = normalized;
            }
        }

        return detectedGroup;
    }

    private static void AddAttributeMeasurements(XElement element, string currentPath, List<MeasurementValue> measurements)
    {
        foreach (var attribute in element.Attributes())
        {
            var value = attribute.Value.Trim();
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            measurements.Add(new MeasurementValue(
                SourcePath: $"{currentPath}/@{attribute.Name.LocalName}",
                DisplayName: $"@{attribute.Name.LocalName}",
                Value: value,
                Unit: null,
                Eye: DetectEye(currentPath),
                Group: DetectGroup(currentPath)));
        }
    }

    private static XAttribute? GetAttribute(XElement element, string name)
    {
        return element.Attributes()
            .FirstOrDefault(attribute => string.Equals(attribute.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
    }

    private static string? GetAttributeValue(XElement element, string name)
    {
        return GetAttribute(element, name)?.Value;
    }
}
