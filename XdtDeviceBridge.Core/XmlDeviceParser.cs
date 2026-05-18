using System.Globalization;
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

            AddNidekLm7MedistarLines(document.Root, measurements);
        }
        catch (Exception ex) when (ex is System.Xml.XmlException or IOException or UnauthorizedAccessException)
        {
            issues.Add(new DeviceParseIssue(DeviceParseIssueSeverity.Error, $"Failed to parse XML: {ex.Message}", "/", null));
        }

        return new DeviceParseResult(measurements, issues);
    }

    private static void ParseElement(XElement element, string parentPath, List<MeasurementValue> measurements)
    {
        foreach (var currentSegment in BuildSegments(element))
        {
            var currentPath = string.IsNullOrEmpty(parentPath) ? currentSegment : $"{parentPath}/{currentSegment}";

            AddAttributeMeasurements(element, currentPath, measurements);

            if (!element.HasElements)
            {
                var value = (element.Value ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    AddMeasurement(
                        measurements,
                        currentPath,
                        element.Name.LocalName,
                        value,
                        GetAttributeValue(element, "Unit"),
                        DetectEye(currentPath),
                        DetectGroup(currentPath));
                }

                continue;
            }

            foreach (var child in element.Elements())
            {
                ParseElement(child, currentPath, measurements);
            }
        }
    }

    private static IReadOnlyList<string> BuildSegments(XElement element)
    {
        var segments = new List<string>();
        AddUniqueSegment(segments, BuildPrimarySegment(element));

        foreach (var alias in GetSegmentAliases(element))
        {
            AddUniqueSegment(segments, alias);
        }

        return segments;
    }

    private static string BuildPrimarySegment(XElement element)
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

    private static IEnumerable<string> GetSegmentAliases(XElement element)
    {
        var typeAttribute = GetAttribute(element, "Type");
        if (typeAttribute is not null)
        {
            var primaryName = typeAttribute.Name.LocalName;
            var aliasName = string.Equals(primaryName, "Type", StringComparison.Ordinal)
                ? "type"
                : "Type";
            yield return $"{element.Name.LocalName}[@{aliasName}='{typeAttribute.Value}']";
        }

        foreach (var alias in GetElementNameAliases(element.Name.LocalName))
        {
            yield return alias;
        }
    }

    private static IEnumerable<string> GetElementNameAliases(string localName)
    {
        if (string.Equals(localName, "Sphare", StringComparison.OrdinalIgnoreCase))
        {
            yield return "Sphere";
        }
        else if (string.Equals(localName, "Sphere", StringComparison.OrdinalIgnoreCase))
        {
            yield return "Sphare";
        }
        else if (string.Equals(localName, "NearSphare", StringComparison.OrdinalIgnoreCase))
        {
            yield return "NearSphere";
        }
        else if (string.Equals(localName, "NearSphere", StringComparison.OrdinalIgnoreCase))
        {
            yield return "NearSphare";
        }
        else if (string.Equals(localName, "NearSphare2", StringComparison.OrdinalIgnoreCase))
        {
            yield return "NearSphere2";
        }
        else if (string.Equals(localName, "NearSphere2", StringComparison.OrdinalIgnoreCase))
        {
            yield return "NearSphare2";
        }
    }

    private static void AddUniqueSegment(List<string> segments, string segment)
    {
        if (!segments.Contains(segment, StringComparer.Ordinal))
        {
            segments.Add(segment);
        }
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

            AddMeasurement(
                measurements,
                $"{currentPath}/@{attribute.Name.LocalName}",
                $"@{attribute.Name.LocalName}",
                value,
                null,
                DetectEye(currentPath),
                DetectGroup(currentPath));
        }
    }

    private static void AddNidekLm7MedistarLines(XElement root, List<MeasurementValue> measurements)
    {
        var common = FindChild(root, "Common");
        var measure = FindMeasure(root, "LM");
        if (common is null || measure is null)
        {
            return;
        }

        var company = GetChildValue(common, "Company");
        var modelName = GetChildValue(common, "ModelName");
        if (!string.Equals(company, "NIDEK", StringComparison.OrdinalIgnoreCase)
            || !IsNidekLm7Model(modelName))
        {
            return;
        }

        var lm = FindChild(measure, "LM");
        if (lm is null)
        {
            return;
        }

        var pd = FindChild(measure, "PD");
        var distance = pd is null ? null : GetChildValue(pd, "Distance");

        AddLensmeterLine(measurements, "R", FindChild(lm, "R"), distance);
        AddLensmeterLine(measurements, "L", FindChild(lm, "L"), null);
    }

    private static bool IsNidekLm7Model(string? modelName)
    {
        var normalized = modelName?.Trim().Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
        return string.Equals(normalized, "LM7", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "LM7P", StringComparison.OrdinalIgnoreCase);
    }

    private static void AddLensmeterLine(
        List<MeasurementValue> measurements,
        string eye,
        XElement? eyeElement,
        string? binocularPd)
    {
        if (eyeElement is null)
        {
            return;
        }

        var sphere = GetChildValue(eyeElement, "Sphere", "Sphare");
        var cylinder = GetChildValue(eyeElement, "Cylinder");
        var axis = GetChildValue(eyeElement, "Axis");
        if (string.IsNullOrWhiteSpace(sphere)
            || string.IsNullOrWhiteSpace(cylinder)
            || string.IsNullOrWhiteSpace(axis))
        {
            return;
        }

        var line = $"{eye}.:S={FormatSignedDecimal(sphere)} Z={FormatSignedDecimal(cylinder)}*{FormatAxis(axis)}";
        line = AppendAddition(line, "A", GetChildValue(eyeElement, "ADD"));
        line = AppendAddition(line, "A2", GetChildValue(eyeElement, "ADD2"));
        line = AppendPrism(line, eyeElement);

        if (!string.IsNullOrWhiteSpace(binocularPd))
        {
            line += $" PD= {FormatPd(binocularPd)}";
        }

        AddMeasurement(
            measurements,
            $"Measure[@Type='LM']/LM/{eye}/MedistarLine",
            $"{eye} MEDISTAR Lensmeter-Zeile",
            line,
            null,
            eye,
            "LM");
    }

    private static string AppendAddition(string line, string label, string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? line
            : $"{line} {label}={FormatSignedDecimal(value)}";
    }

    private static string AppendPrism(string line, XElement eyeElement)
    {
        var prismX = GetChildValue(eyeElement, "PrismX");
        var prismY = GetChildValue(eyeElement, "PrismY");
        if (string.IsNullOrWhiteSpace(prismX) || string.IsNullOrWhiteSpace(prismY))
        {
            return line;
        }

        var prismXBase = NormalizePrismBase(FindChild(eyeElement, "PrismX")?.Attributes()
            .FirstOrDefault(attribute => string.Equals(attribute.Name.LocalName, "base", StringComparison.OrdinalIgnoreCase))
            ?.Value);
        var prismYBase = NormalizePrismBase(FindChild(eyeElement, "PrismY")?.Attributes()
            .FirstOrDefault(attribute => string.Equals(attribute.Name.LocalName, "base", StringComparison.OrdinalIgnoreCase))
            ?.Value);
        if (string.IsNullOrWhiteSpace(prismXBase) || string.IsNullOrWhiteSpace(prismYBase))
        {
            return line;
        }

        return $"{line} P={FormatUnsignedDecimal(prismX)} {prismXBase} {FormatUnsignedDecimal(prismY)} {prismYBase}";
    }

    private static string? NormalizePrismBase(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "out" => "OUT",
            "in" => "IN",
            "up" => "UP",
            "down" => "DOWN",
            _ => null
        };
    }

    private static string FormatSignedDecimal(string value)
    {
        if (!decimal.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            return value.Trim();
        }

        var sign = number < 0 ? "- " : "+ ";
        return sign + Math.Abs(number).ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string FormatUnsignedDecimal(string value)
    {
        if (!decimal.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            return value.Trim();
        }

        return Math.Abs(number).ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string FormatAxis(string value)
    {
        if (decimal.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var axis))
        {
            return ((int)Math.Round(axis, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture).PadLeft(3);
        }

        return value.Trim().PadLeft(3);
    }

    private static string FormatPd(string value)
    {
        if (!decimal.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            return value.Trim();
        }

        return number % 1 == 0
            ? number.ToString("0", CultureInfo.InvariantCulture)
            : number.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static XElement? FindMeasure(XElement root, string type)
    {
        return root.Elements()
            .FirstOrDefault(element =>
                string.Equals(element.Name.LocalName, "Measure", StringComparison.OrdinalIgnoreCase)
                && element.Attributes().Any(attribute =>
                    string.Equals(attribute.Name.LocalName, "Type", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(attribute.Value, type, StringComparison.OrdinalIgnoreCase)));
    }

    private static XElement? FindChild(XElement element, string name)
    {
        return element.Elements()
            .FirstOrDefault(child => string.Equals(child.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
    }

    private static string? GetChildValue(XElement element, params string[] names)
    {
        foreach (var name in names)
        {
            var value = FindChild(element, name)?.Value.Trim();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static void AddMeasurement(
        List<MeasurementValue> measurements,
        string sourcePath,
        string displayName,
        string value,
        string? unit,
        string? eye,
        string? group)
    {
        measurements.Add(new MeasurementValue(
            SourcePath: sourcePath,
            DisplayName: displayName,
            Value: value,
            Unit: unit,
            Eye: eye,
            Group: group));
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
