using System.Globalization;
using System.Xml.Linq;

namespace XdtDeviceBridge.Core;

public sealed class XmlDeviceParser
{
    private static readonly HashSet<string> KnownGroups = new(StringComparer.OrdinalIgnoreCase)
    {
        "ARMedian", "ARList", "TrialLens", "ContactLens", "PDList", "LM", "PD", "NT", "PACHY", "CorrectedIOP"
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
            AddNidekNt530PMedistarLines(document.Root, measurements);
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

    private static void AddNidekNt530PMedistarLines(XElement root, List<MeasurementValue> measurements)
    {
        if (!string.Equals(root.Name.LocalName, "Data", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var company = GetChildValue(root, "Company");
        var modelName = GetChildValue(root, "ModelName");
        if (!string.Equals(company, "NIDEK", StringComparison.OrdinalIgnoreCase)
            || !IsNidekNt530PModel(modelName))
        {
            return;
        }

        var right = FindChild(root, "R");
        var left = FindChild(root, "L");
        var time = GetChildValue(root, "Time");

        var pachyLine = BuildNt530PPachyMedistarLine(right, left);
        if (!string.IsNullOrWhiteSpace(pachyLine))
        {
            AddMeasurement(
                measurements,
                "Measure[@Type='NT530P']/Pachy/MedistarLine",
                "NT530P MEDISTAR Pachymetrie-Zeile",
                pachyLine,
                null,
                null,
                "PACHY");
        }

        var tonoLine = BuildNt530PTonoMedistarLine(right, left, time);
        if (!string.IsNullOrWhiteSpace(tonoLine))
        {
            AddMeasurement(
                measurements,
                "Measure[@Type='NT530P']/Tono/MedistarLine",
                "NT530P MEDISTAR Tonometrie-Zeile",
                tonoLine,
                null,
                null,
                "NT");
        }
    }

    private static bool IsNidekNt530PModel(string? modelName)
    {
        var normalized = modelName?.Trim().Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
        return string.Equals(normalized, "NT530P", StringComparison.OrdinalIgnoreCase);
    }

    private static string? BuildNt530PPachyMedistarLine(XElement? right, XElement? left)
    {
        var parts = new List<string>();
        var rightAverage = GetNt530PPachyAverage(right);
        var leftAverage = GetNt530PPachyAverage(left);
        if (!string.IsNullOrWhiteSpace(rightAverage))
        {
            parts.Add($"RA: {FormatMicrometersAsMillimeters(rightAverage)}");
        }

        if (!string.IsNullOrWhiteSpace(leftAverage))
        {
            parts.Add($"LA: {FormatMicrometersAsMillimeters(leftAverage)}");
        }

        return parts.Count == 0 ? null : string.Join("   // ", parts);
    }

    private static string? BuildNt530PTonoMedistarLine(XElement? right, XElement? left, string? time)
    {
        var parts = new List<string>();

        var rightPachy = BuildNt530PPachyListSegment("PR", right);
        if (!string.IsNullOrWhiteSpace(rightPachy))
        {
            parts.Add(rightPachy);
        }

        var leftPachy = BuildNt530PPachyListSegment("PL", left);
        if (!string.IsNullOrWhiteSpace(leftPachy))
        {
            parts.Add(leftPachy);
        }

        var correctedParts = new List<string>();
        AddNt530PCorrectedIopSegments(correctedParts, "PR", right);
        AddNt530PCorrectedIopSegments(correctedParts, "PL", left);
        if (correctedParts.Count > 0)
        {
            parts.Add(string.Join(" Y  ", correctedParts));
        }

        var tonometryList = BuildNt530PTonometryListSegment(right, left);
        if (!string.IsNullOrWhiteSpace(tonometryList))
        {
            parts.Add($"P  {tonometryList}");
        }

        var formattedTime = FormatNt530PTime(time);
        if (!string.IsNullOrWhiteSpace(formattedTime))
        {
            parts.Add($"{formattedTime} / EV:{{000000003B}} NT-530P Messung");
        }
        else if (parts.Count > 0)
        {
            parts.Add("EV:{000000003B} NT-530P Messung");
        }

        return parts.Count == 0 ? null : string.Join(" ", parts);
    }

    private static string? BuildNt530PPachyListSegment(string label, XElement? eyeElement)
    {
        var pachy = eyeElement is null ? null : FindChild(eyeElement, "PACHY");
        if (pachy is null)
        {
            return null;
        }

        var values = FindChildren(pachy, "PACHYList")
            .Select(element => GetChildValue(element, "Thickness"))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToList();
        var average = GetNt530PPachyAverage(eyeElement);

        if (values.Count == 0 && string.IsNullOrWhiteSpace(average))
        {
            return null;
        }

        var valuePart = values.Count == 0 ? string.Empty : string.Join(" ", values.Select(FormatPlainNumber));
        var averagePart = string.IsNullOrWhiteSpace(average) ? string.Empty : $"[{FormatPlainNumber(average)}]";
        return $"{label}: {string.Join(" ", new[] { valuePart, averagePart }.Where(part => !string.IsNullOrWhiteSpace(part)))} µm";
    }

    private static void AddNt530PCorrectedIopSegments(List<string> parts, string label, XElement? eyeElement)
    {
        var nt = eyeElement is null ? null : FindChild(eyeElement, "NT");
        var correctedIop = nt is null ? null : FindChild(nt, "CorrectedIOP");
        if (correctedIop is null)
        {
            return;
        }

        var measured = GetChildValue(FindChild(correctedIop, "Measured") ?? new XElement("Measured"), "mmHg");
        var corrected = GetChildValue(FindChild(correctedIop, "Corrected") ?? new XElement("Corrected"), "mmHg");
        if (!string.IsNullOrWhiteSpace(measured) || !string.IsNullOrWhiteSpace(corrected))
        {
            var measuredPart = string.IsNullOrWhiteSpace(measured) ? null : $"Gemessen = {FormatOneDecimal(measured)} mmHg";
            var correctedPart = string.IsNullOrWhiteSpace(corrected) ? null : $"Korrigiert = {FormatOneDecimal(corrected)} mmHg";
            parts.Add($"{label}: {string.Join("; ", new[] { measuredPart, correctedPart }.Where(part => !string.IsNullOrWhiteSpace(part)))}");
        }

        var param1 = GetChildValue(correctedIop, "Param1");
        var param2 = GetChildValue(correctedIop, "Param2");
        var cct = GetChildValue(correctedIop, "CCT");
        var parameterParts = new[]
            {
                string.IsNullOrWhiteSpace(param1) ? null : $"Param1 = {param1}",
                string.IsNullOrWhiteSpace(param2) ? null : $"Param2 = {param2}",
                string.IsNullOrWhiteSpace(cct) ? null : $"CCT = {cct}"
            }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToList();
        if (parameterParts.Count > 0)
        {
            parts.Add($"{label}: {string.Join("; ", parameterParts)}");
        }
    }

    private static string? BuildNt530PTonometryListSegment(XElement? right, XElement? left)
    {
        var parts = new List<string>();
        var rightSegment = BuildNt530PEyeTonometryList("R", right);
        var leftSegment = BuildNt530PEyeTonometryList("L", left);
        if (!string.IsNullOrWhiteSpace(rightSegment))
        {
            parts.Add(rightSegment);
        }

        if (!string.IsNullOrWhiteSpace(leftSegment))
        {
            parts.Add(leftSegment);
        }

        return parts.Count == 0 ? null : $"{string.Join(" // ", parts)} mmHg";
    }

    private static string? BuildNt530PEyeTonometryList(string label, XElement? eyeElement)
    {
        var nt = eyeElement is null ? null : FindChild(eyeElement, "NT");
        if (nt is null)
        {
            return null;
        }

        var values = FindChildren(nt, "NTList")
            .Select(element => GetChildValue(element, "mmHg"))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToList();
        var average = GetChildValue(FindChild(nt, "NTAverage") ?? new XElement("NTAverage"), "mmHg");
        if (values.Count == 0 && string.IsNullOrWhiteSpace(average))
        {
            return null;
        }

        var valuePart = values.Count == 0 ? string.Empty : string.Join(" ", values.Select(FormatPlainNumber));
        var averagePart = string.IsNullOrWhiteSpace(average) ? string.Empty : $"[{FormatOneDecimal(average)}]";
        return $"{label} = {string.Join(" ", new[] { valuePart, averagePart }.Where(part => !string.IsNullOrWhiteSpace(part)))}";
    }

    private static string? GetNt530PPachyAverage(XElement? eyeElement)
    {
        var pachy = eyeElement is null ? null : FindChild(eyeElement, "PACHY");
        return pachy is null ? null : GetChildValue(FindChild(pachy, "PACHYAverage") ?? new XElement("PACHYAverage"), "Thickness");
    }

    private static string FormatMicrometersAsMillimeters(string value)
    {
        if (!TryParseDecimal(value, out var micrometers))
        {
            return value.Trim();
        }

        return (micrometers / 1000m).ToString("0.000", CultureInfo.InvariantCulture);
    }

    private static string FormatPlainNumber(string value)
    {
        if (!TryParseDecimal(value, out var number))
        {
            return value.Trim();
        }

        return number % 1 == 0
            ? number.ToString("0", CultureInfo.InvariantCulture)
            : number.ToString("0.#", CultureInfo.InvariantCulture);
    }

    private static string FormatOneDecimal(string value)
    {
        if (!TryParseDecimal(value, out var number))
        {
            return value.Trim();
        }

        return number.ToString("0.0", CultureInfo.InvariantCulture);
    }

    private static string? FormatNt530PTime(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        if (TimeSpan.TryParse(trimmed, CultureInfo.InvariantCulture, out var time))
        {
            return $"{time.Hours:00}:{time.Minutes:00}";
        }

        return trimmed.Length >= 5 ? trimmed[..5] : trimmed;
    }

    private static bool TryParseDecimal(string value, out decimal number)
    {
        var normalized = value.Trim()
            .Replace("um", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("µm", string.Empty, StringComparison.OrdinalIgnoreCase);
        return decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
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

    private static IEnumerable<XElement> FindChildren(XElement element, string name)
    {
        return element.Elements()
            .Where(child => string.Equals(child.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
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
