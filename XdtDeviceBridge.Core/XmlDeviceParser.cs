using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace XdtDeviceBridge.Core;

public sealed class XmlDeviceParser
{
    private static readonly HashSet<string> KnownGroups = new(StringComparer.OrdinalIgnoreCase)
    {
        "ARMedian", "ARList", "TrialLens", "ContactLens", "PDList", "LM", "PD", "NT", "PACHY", "CorrectedIOP",
        "REF", "KM", "TM", "CCT", "SBJ"
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
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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
            AddTopconCl300MedistarLines(document.Root, measurements);
            AddTopconKr800SMedistarLines(document.Root, measurements);
            AddTopconTrk2PMedistarLines(document.Root, measurements);
            AddTopconCt1PMedistarLines(document.Root, measurements);
            AddTopconCv5000MedistarLines(document.Root, measurements);
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

    private static void AddTopconCl300MedistarLines(XElement root, List<MeasurementValue> measurements)
    {
        if (!string.Equals(root.Name.LocalName, "Ophthalmology", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var common = FindChild(root, "Common");
        var measure = FindMeasure(root, "LM");
        if (common is null || measure is null)
        {
            return;
        }

        var company = GetChildValue(common, "Company");
        var modelName = GetChildValue(common, "ModelName");
        if (!string.Equals(company, "TOPCON", StringComparison.OrdinalIgnoreCase)
            || !IsTopconCl300Model(modelName))
        {
            return;
        }

        var lm = FindChild(measure, "LM");
        if (lm is null)
        {
            return;
        }

        var pd = FindChild(measure, "PD");
        var binocularPd = pd is null
            ? null
            : GetChildValue(FindChild(pd, "B") ?? pd, "Distance");

        AddLensmeterLine(measurements, "R", FindChild(lm, "R"), binocularPd, includeSignedPrismComponents: true);
        AddLensmeterLine(measurements, "L", FindChild(lm, "L"), null, includeSignedPrismComponents: true);
    }

    private static void AddTopconKr800SMedistarLines(XElement root, List<MeasurementValue> measurements)
    {
        if (!string.Equals(root.Name.LocalName, "Ophthalmology", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var common = FindChild(root, "Common");
        if (common is null)
        {
            return;
        }

        var company = GetChildValue(common, "Company");
        var modelName = GetChildValue(common, "ModelName");
        if (!string.Equals(company, "TOPCON", StringComparison.OrdinalIgnoreCase)
            || !IsTopconKr800SModel(modelName))
        {
            return;
        }

        var refMeasure = FindMeasure(root, "REF");
        if (refMeasure is not null)
        {
            AddTopconKr800SRefMedistarLines(measurements, refMeasure);
        }

        var kmMeasure = FindMeasure(root, "KM");
        if (kmMeasure is not null)
        {
            AddTopconKr800SKmMedistarLines(measurements, kmMeasure);
        }

        var sbjMeasure = FindMeasure(root, "SBJ");
        if (sbjMeasure is not null)
        {
            AddTopconKr800SSbjMedistarLines(measurements, sbjMeasure);
        }
    }

    private static void AddTopconTrk2PMedistarLines(XElement root, List<MeasurementValue> measurements)
    {
        if (!string.Equals(root.Name.LocalName, "Ophthalmology", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var common = FindChild(root, "Common");
        if (common is null)
        {
            return;
        }

        var company = GetChildValue(common, "Company");
        var modelName = GetChildValue(common, "ModelName");
        if (!string.Equals(company, "TOPCON", StringComparison.OrdinalIgnoreCase)
            || !IsTopconTrk2PModel(modelName))
        {
            return;
        }

        var refMeasure = FindMeasure(root, "REF");
        if (refMeasure is not null)
        {
            AddTopconKr800SRefMedistarLines(measurements, refMeasure);
        }

        var kmMeasure = FindMeasure(root, "KM");
        if (kmMeasure is not null)
        {
            AddTopconKr800SKmMedistarLines(measurements, kmMeasure);
        }

        var tmMeasure = FindMeasure(root, "TM");
        if (tmMeasure is not null)
        {
            AddTopconTrk2PTonoAndPachyMedistarLines(measurements, tmMeasure, FindMeasure(root, "CCT") ?? tmMeasure, GetChildValue(common, "Time"));
        }

        var sbjMeasure = FindMeasure(root, "SBJ");
        if (sbjMeasure is not null)
        {
            AddTopconKr800SSbjMedistarLines(measurements, sbjMeasure);
        }
    }

    private static void AddTopconCt1PMedistarLines(XElement root, List<MeasurementValue> measurements)
    {
        if (!string.Equals(root.Name.LocalName, "Ophthalmology", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var common = FindChild(root, "Common");
        if (common is null)
        {
            return;
        }

        var company = GetChildValue(common, "Company");
        var modelName = GetChildValue(common, "ModelName");
        if (!string.Equals(company, "TOPCON", StringComparison.OrdinalIgnoreCase)
            || !IsTopconCt1PModel(modelName))
        {
            return;
        }

        var tmMeasure = FindMeasure(root, "TM");
        if (tmMeasure is null)
        {
            return;
        }

        AddTopconCt1PTonoAndPachyMedistarLines(measurements, tmMeasure, GetChildValue(common, "Time"));
    }

    private static void AddTopconCv5000MedistarLines(XElement root, List<MeasurementValue> measurements)
    {
        if (!string.Equals(root.Name.LocalName, "Ophthalmology", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var common = FindChild(root, "Common");
        var company = common is null ? null : GetChildValue(common, "Company");
        var modelName = common is null ? null : GetChildValue(common, "ModelName");
        if (!string.Equals(company?.Trim(), "TOPCON", StringComparison.OrdinalIgnoreCase)
            || !IsTopconCv5000Model(modelName))
        {
            return;
        }

        var sbjMeasure = FindMeasure(root, "SBJ");
        if (sbjMeasure is null)
        {
            return;
        }

        var refractionTest = FindChild(sbjMeasure, "RefractionTest");
        if (refractionTest is null)
        {
            return;
        }

        foreach (var type in FindChildren(refractionTest, "Type"))
        {
            var typeName = GetChildValue(type, "TypeName");
            if (IsCv5000PrescriptionType(typeName))
            {
                AddTopconCv5000TypeMedistarLines(
                    measurements,
                    type,
                    "Prescription",
                    "Phoropter finaler Verordnungswert");
            }
            else if (IsCv5000FullCorrectionType(typeName))
            {
                AddTopconCv5000TypeMedistarLines(
                    measurements,
                    type,
                    "FullCorrection",
                    "Phoropter Maximalwert (Vollkorrektion)");
            }
        }
    }

    private static void AddTopconCv5000TypeMedistarLines(
        List<MeasurementValue> measurements,
        XElement type,
        string pathSegment,
        string headerLine)
    {
        string? rightLine = null;
        string? leftLine = null;

        foreach (var examDistance in FindChildren(type, "ExamDistance"))
        {
            var refractionData = FindChild(examDistance, "RefractionData");
            if (refractionData is null)
            {
                continue;
            }

            var pd = GetCv5000BinocularPd(FindChild(examDistance, "PD"));
            var vd = GetChildValue(refractionData, "VD");
            rightLine ??= BuildCv5000EyeLine("R", FindChild(refractionData, "R"), pd, vd);
            leftLine ??= BuildCv5000EyeLine("L", FindChild(refractionData, "L"), null, null);

            if (!string.IsNullOrWhiteSpace(rightLine) && !string.IsNullOrWhiteSpace(leftLine))
            {
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(rightLine) && string.IsNullOrWhiteSpace(leftLine))
        {
            return;
        }

        AddMeasurement(
            measurements,
            $"Measure[@Type='SBJ']/{pathSegment}/HeaderLine",
            $"MEDISTAR TOPCON CV-5000 {pathSegment}-Header",
            headerLine,
            null,
            null,
            "SBJ");

        AddCv5000OptionalMedistarLine(measurements, pathSegment, "R", rightLine);
        AddCv5000OptionalMedistarLine(measurements, pathSegment, "L", leftLine);
    }

    private static void AddCv5000OptionalMedistarLine(
        List<MeasurementValue> measurements,
        string pathSegment,
        string eye,
        string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        AddMeasurement(
            measurements,
            $"Measure[@Type='SBJ']/{pathSegment}/{eye}/MedistarLine",
            $"MEDISTAR TOPCON CV-5000 {pathSegment} {eye}-Zeile",
            line,
            null,
            null,
            "SBJ");
    }

    private static void AddTopconKr800SRefMedistarLines(List<MeasurementValue> measurements, XElement refMeasure)
    {
        var refRoot = FindChild(refMeasure, "REF");
        if (refRoot is null)
        {
            return;
        }

        var pd = GetChildValue(FindChild(refMeasure, "PD") ?? new XElement("PD"), "Distance");
        var vd = GetChildValue(refMeasure, "VD");

        AddTopconKr800SRefEyeLine(measurements, "R", FindChild(FindChild(refRoot, "R") ?? new XElement("R"), "Median"), pd, vd);
        AddTopconKr800SRefEyeLine(measurements, "L", FindChild(FindChild(refRoot, "L") ?? new XElement("L"), "Median"), null, null);
    }

    private static void AddTopconKr800SRefEyeLine(
        List<MeasurementValue> measurements,
        string eye,
        XElement? median,
        string? pd,
        string? vd)
    {
        var line = BuildRefractionEyeLine(eye, median);
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(pd))
        {
            line += $" PD= {FormatPd(pd)}";
        }

        if (!string.IsNullOrWhiteSpace(vd))
        {
            line += $" VD= {FormatTwoDecimal(vd)}";
        }

        AddMeasurement(
            measurements,
            $"Measure[@Type='REF']/REF/{eye}/MedistarLine",
            $"{eye} MEDISTAR TOPCON KR-800S REF-Zeile",
            line,
            null,
            eye,
            "REF");
    }

    private static void AddTopconKr800SKmMedistarLines(List<MeasurementValue> measurements, XElement kmMeasure)
    {
        var kmRoot = FindChild(kmMeasure, "KM");
        if (kmRoot is null)
        {
            return;
        }

        var rightMedian = FindChild(FindChild(kmRoot, "R") ?? new XElement("R"), "Median");
        var leftMedian = FindChild(FindChild(kmRoot, "L") ?? new XElement("L"), "Median");

        var radiiParts = new[]
            {
                BuildKeratometryRadiiSegment("R", rightMedian),
                BuildKeratometryRadiiSegment("L", leftMedian)
            }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!)
            .ToArray();
        if (radiiParts.Length > 0)
        {
            AddMeasurement(
                measurements,
                "Measure[@Type='KM']/KM/MedistarLine1",
                "MEDISTAR TOPCON KR-800S KM R1/R2-Zeile",
                string.Join(" // ", radiiParts),
                null,
                null,
                "KM");
        }

        var averageParts = new[]
            {
                BuildKeratometryAverageSegment("R", rightMedian),
                BuildKeratometryAverageSegment("L", leftMedian)
            }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!)
            .ToArray();
        if (averageParts.Length > 0)
        {
            AddMeasurement(
                measurements,
                "Measure[@Type='KM']/KM/MedistarLine2",
                "MEDISTAR TOPCON KR-800S KM AV/CYL-Zeile",
                string.Join(" // ", averageParts),
                null,
                null,
                "KM");
        }
    }

    private static void AddTopconKr800SSbjMedistarLines(List<MeasurementValue> measurements, XElement sbjMeasure)
    {
        var refractionTest = FindChild(sbjMeasure, "RefractionTest");
        if (refractionTest is null)
        {
            return;
        }

        var lines = new List<string>();
        foreach (var type in FindChildren(refractionTest, "Type"))
        {
            var typeName = GetChildValue(type, "TypeName") ?? "Subjektive Refraktion";
            foreach (var examDistance in FindChildren(type, "ExamDistance"))
            {
                var examLines = BuildSubjectiveRefractionLines(typeName, examDistance);
                if (examLines.Count > 0)
                {
                    lines.AddRange(examLines);
                }
            }
        }

        for (var i = 0; i < lines.Count; i++)
        {
            AddMeasurement(
                measurements,
                $"Measure[@Type='SBJ']/MedistarLine{i + 1}",
                $"MEDISTAR TOPCON KR-800S SBJ-Zeile {i + 1}",
                lines[i],
                null,
                null,
                "SBJ");
        }
    }

    private static void AddTopconTrk2PTonoAndPachyMedistarLines(
        List<MeasurementValue> measurements,
        XElement tmMeasure,
        XElement? cctMeasure,
        string? time)
    {
        var tmRoot = FindChild(tmMeasure, "TM");
        if (tmRoot is null)
        {
            return;
        }

        var corrected = GetTopconTrk2PCorrectedIopParts(tmMeasure);
        var pachyLine = BuildTopconTrk2PPachyMedistarLine(cctMeasure, corrected.Right, corrected.Left);
        if (!string.IsNullOrWhiteSpace(pachyLine))
        {
            AddMeasurement(
                measurements,
                "Measure[@Type='CCT']/Pachy/HeaderLine",
                "TOPCON TRK-2P MEDISTAR Pachymetrie-Überschrift",
                "Pachymetrie",
                null,
                null,
                "CCT");
            AddMeasurement(
                measurements,
                "Measure[@Type='CCT']/Pachy/MedistarLine",
                "TOPCON TRK-2P MEDISTAR Pachymetrie-Zeile",
                pachyLine,
                null,
                null,
                "CCT");
        }

        var tonoLines = BuildTopconTrk2PTonoMedistarLines(tmRoot, cctMeasure, corrected.Right, corrected.Left, time);
        foreach (var line in tonoLines)
        {
            AddMeasurement(
                measurements,
                $"Measure[@Type='TM']/Tono/{line.Key}",
                line.DisplayName,
                line.Value,
                null,
                null,
                "TM");
        }
    }

    private static void AddTopconCt1PTonoAndPachyMedistarLines(
        List<MeasurementValue> measurements,
        XElement tmMeasure,
        string? time)
    {
        var tmRoot = FindChild(tmMeasure, "TM");
        if (tmRoot is null)
        {
            return;
        }

        var corrected = GetTopconTrk2PCorrectedIopParts(tmMeasure);
        var rightCorrected = SuppressTopconCt1PParameterOnlyCorrectedIop(corrected.Right);
        var leftCorrected = SuppressTopconCt1PParameterOnlyCorrectedIop(corrected.Left);
        var pachyLine = BuildTopconTrk2PPachyMedistarLine(null, rightCorrected, leftCorrected);
        if (!string.IsNullOrWhiteSpace(pachyLine))
        {
            AddMeasurement(
                measurements,
                "Measure[@Type='CCT']/Pachy/HeaderLine",
                "TOPCON CT-1P MEDISTAR Pachymetrie-Überschrift",
                "Pachymetrie",
                null,
                null,
                "CCT");
            AddMeasurement(
                measurements,
                "Measure[@Type='CCT']/Pachy/MedistarLine",
                "TOPCON CT-1P MEDISTAR Pachymetrie-Zeile",
                pachyLine,
                null,
                null,
                "CCT");
        }

        var tonoLines = BuildTopconTrk2PTonoMedistarLines(tmRoot, null, rightCorrected, leftCorrected, time);
        foreach (var line in tonoLines)
        {
            AddMeasurement(
                measurements,
                $"Measure[@Type='TM']/Tono/{line.Key}",
                line.DisplayName.Replace("TRK-2P", "CT-1P", StringComparison.Ordinal),
                line.Value,
                null,
                null,
                "TM");
        }
    }

    private static TopconTrk2PCorrectedIopParts SuppressTopconCt1PParameterOnlyCorrectedIop(TopconTrk2PCorrectedIopParts parts)
    {
        return string.IsNullOrWhiteSpace(parts.Cct)
            && string.IsNullOrWhiteSpace(parts.Measured)
            && string.IsNullOrWhiteSpace(parts.Corrected)
            ? new TopconTrk2PCorrectedIopParts(null, null, null, null, null)
            : parts;
    }

    private static string? BuildTopconTrk2PPachyMedistarLine(
        XElement? cctMeasure,
        TopconTrk2PCorrectedIopParts rightCorrected,
        TopconTrk2PCorrectedIopParts leftCorrected)
    {
        var rightCct = GetTopconTrk2PCctValues(cctMeasure, "R").SelectedMillimeters ?? rightCorrected.Cct;
        var leftCct = GetTopconTrk2PCctValues(cctMeasure, "L").SelectedMillimeters ?? leftCorrected.Cct;

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(rightCct))
        {
            parts.Add($"RA: {FormatThreeDecimal(rightCct)}");
        }

        if (!string.IsNullOrWhiteSpace(leftCct))
        {
            parts.Add($"LA: {FormatThreeDecimal(leftCct)}");
        }

        return parts.Count == 0 ? null : string.Join("   // ", parts);
    }

    private static TopconTrk2PCctValues GetTopconTrk2PCctValues(XElement? cctMeasure, string eye)
    {
        if (cctMeasure is null)
        {
            return TopconTrk2PCctValues.Empty;
        }

        var cctRoot = string.Equals(cctMeasure.Name.LocalName, "CCT", StringComparison.OrdinalIgnoreCase)
            ? cctMeasure
            : FindChild(cctMeasure, "CCT");
        var eyeElement = cctRoot is null ? null : FindChild(cctRoot, eye);
        if (eyeElement is null)
        {
            return TopconTrk2PCctValues.Empty;
        }

        var average = FindChild(eyeElement, "Average");
        var averageValue = average is null ? null : GetChildValue(average, "CCT_mm");
        var values = FindChildren(eyeElement, "List")
            .Select(element => GetChildValue(element, "CCT_mm"))
            .Where(value => !string.IsNullOrWhiteSpace(value) && TryParseDecimal(value, out _))
            .Select(value => value!.Trim())
            .ToArray();
        if (!string.IsNullOrWhiteSpace(averageValue))
        {
            var averageTrimmed = averageValue.Trim();
            return new TopconTrk2PCctValues(
                averageTrimmed,
                values.Length == 0 ? new[] { averageTrimmed } : values);
        }

        return values.Length == 0
            ? TopconTrk2PCctValues.Empty
            : new TopconTrk2PCctValues(CalculateTopconTrk2PCctAverage(values), values);
    }

    private static IReadOnlyList<TopconTrk2PMedistarLine> BuildTopconTrk2PTonoMedistarLines(
        XElement tmRoot,
        XElement? cctMeasure,
        TopconTrk2PCorrectedIopParts rightCorrected,
        TopconTrk2PCorrectedIopParts leftCorrected,
        string? time)
    {
        var lines = new List<TopconTrk2PMedistarLine>();

        var rightCct = GetTopconTrk2PCctValues(cctMeasure, "R");
        var leftCct = GetTopconTrk2PCctValues(cctMeasure, "L");
        var rightPachy = BuildTopconTrk2PCorrectedPachyLine(
            "PR",
            rightCct,
            rightCorrected.Cct);
        if (!string.IsNullOrWhiteSpace(rightPachy))
        {
            lines.Add(new TopconTrk2PMedistarLine(
                "PachyRightLine",
                "TOPCON TRK-2P MEDISTAR Tonometrie Pachymetrie rechts",
                rightPachy));
        }

        var leftPachy = BuildTopconTrk2PCorrectedPachyLine(
            "PL",
            leftCct,
            leftCorrected.Cct);
        if (!string.IsNullOrWhiteSpace(leftPachy))
        {
            lines.Add(new TopconTrk2PMedistarLine(
                "PachyLeftLine",
                "TOPCON TRK-2P MEDISTAR Tonometrie Pachymetrie links",
                leftPachy));
        }

        var tonometryList = BuildTopconTrk2PTonometryListSegment(tmRoot, time);

        var rightMeasured = BuildTopconTrk2PCorrectedIopMeasurementLine("PR", rightCorrected);
        if (!string.IsNullOrWhiteSpace(rightMeasured))
        {
            lines.Add(new TopconTrk2PMedistarLine(
                "MeasuredRightLine",
                "TOPCON TRK-2P MEDISTAR Tonometrie Messung rechts",
                rightMeasured));
        }

        var rightParameters = BuildTopconTrk2PCorrectedIopParameterLine("PR", rightCorrected);
        if (!string.IsNullOrWhiteSpace(rightParameters))
        {
            lines.Add(new TopconTrk2PMedistarLine(
                "ParameterRightLine",
                "TOPCON TRK-2P MEDISTAR Tonometrie Parameter rechts",
                rightParameters));
        }

        var leftMeasured = BuildTopconTrk2PCorrectedIopMeasurementLine("PL", leftCorrected);
        if (!string.IsNullOrWhiteSpace(leftMeasured))
        {
            lines.Add(new TopconTrk2PMedistarLine(
                "MeasuredLeftLine",
                "TOPCON TRK-2P MEDISTAR Tonometrie Messung links",
                leftMeasured));
        }

        var leftParameters = BuildTopconTrk2PCorrectedIopParameterLine("PL", leftCorrected);
        if (!string.IsNullOrWhiteSpace(leftParameters))
        {
            lines.Add(new TopconTrk2PMedistarLine(
                "ParameterLeftLine",
                "TOPCON TRK-2P MEDISTAR Tonometrie Parameter links",
                leftParameters));
        }

        if (!string.IsNullOrWhiteSpace(tonometryList))
        {
            lines.Add(new TopconTrk2PMedistarLine(
                "TonoListLine",
                "TOPCON TRK-2P MEDISTAR Tonometrie Einzelwerte",
                tonometryList));
        }

        if (lines.Count > 0)
        {
            lines.Insert(0, new TopconTrk2PMedistarLine(
                "HeaderLine",
                "TOPCON TRK-2P MEDISTAR Tonometrie-Überschrift",
                "Tonometrie"));
        }

        return lines;
    }

    private static string? BuildTopconTrk2PCorrectedPachyLine(string label, TopconTrk2PCctValues cctValues, string? correctedCct)
    {
        if (cctValues.ListMillimeters.Count > 0)
        {
            var valuePart = string.Join(" ", cctValues.ListMillimeters.Select(FormatMillimetersAsMicrometers));
            var selected = string.IsNullOrWhiteSpace(cctValues.SelectedMillimeters)
                ? string.Empty
                : $"[{FormatMillimetersAsMicrometers(cctValues.SelectedMillimeters)}]";
            return $"{label}: {string.Join(" ", new[] { valuePart, selected }.Where(part => !string.IsNullOrWhiteSpace(part)))} µm";
        }

        var cct = cctValues.SelectedMillimeters ?? correctedCct;
        if (string.IsNullOrWhiteSpace(cct))
        {
            return null;
        }

        var micrometers = FormatMillimetersAsMicrometers(cct);
        return $"{label}: {micrometers} [{micrometers}] µm";
    }

    private static string CalculateTopconTrk2PCctAverage(IReadOnlyList<string> values)
    {
        var parsed = values
            .Select(value => TryParseDecimal(value, out var number) ? number : (decimal?)null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToArray();
        if (parsed.Length == 0)
        {
            return string.Empty;
        }

        var average = parsed.Average();
        return average.ToString("0.000", CultureInfo.InvariantCulture);
    }

    private static string? BuildTopconTrk2PCorrectedIopMeasurementLine(string label, TopconTrk2PCorrectedIopParts values)
    {
        var measurementParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(values.Measured))
        {
            measurementParts.Add($"Gemessen = {FormatOneDecimal(values.Measured)} mmHg");
        }

        if (!string.IsNullOrWhiteSpace(values.Corrected))
        {
            measurementParts.Add($"Korrigiert = {FormatOneDecimal(values.Corrected)} mmHg");
        }

        if (measurementParts.Count > 0)
        {
            return $"{label}: {string.Join("; ", measurementParts)};";
        }

        return null;
    }

    private static string? BuildTopconTrk2PCorrectedIopParameterLine(string label, TopconTrk2PCorrectedIopParts values)
    {
        var parameterParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(values.Param1))
        {
            parameterParts.Add($"Param1 = {FormatMillimetersAsMicrometers(values.Param1)}um");
        }

        if (!string.IsNullOrWhiteSpace(values.Param2))
        {
            parameterParts.Add($"Param2 = {values.Param2.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(values.Cct))
        {
            parameterParts.Add($"CCT = {FormatMillimetersAsMicrometers(values.Cct)}um");
        }

        if (parameterParts.Count > 0)
        {
            return $"{label}: {string.Join("; ", parameterParts)}";
        }

        return null;
    }

    private static string? BuildTopconTrk2PTonometryListSegment(XElement tmRoot, string? time)
    {
        var parts = new List<string>();
        var rightSegment = BuildTopconTrk2PEyeTonometryList("R", FindChild(tmRoot, "R"));
        var leftSegment = BuildTopconTrk2PEyeTonometryList("L", FindChild(tmRoot, "L"));
        if (!string.IsNullOrWhiteSpace(rightSegment))
        {
            parts.Add(rightSegment);
        }

        if (!string.IsNullOrWhiteSpace(leftSegment))
        {
            parts.Add(leftSegment);
        }

        if (parts.Count == 0)
        {
            return null;
        }

        var line = $"{string.Join(" // ", parts)} mmHg";
        var formattedTime = FormatNt530PTime(time);
        return string.IsNullOrWhiteSpace(formattedTime) ? line : $"{line} {formattedTime}";
    }

    private static string? BuildTopconTrk2PEyeTonometryList(string label, XElement? eyeElement)
    {
        if (eyeElement is null)
        {
            return null;
        }

        var values = FindChildren(eyeElement, "List")
            .Select(element => GetChildValue(element, "IOP_mmHg"))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToList();
        var average = GetChildValue(FindChild(eyeElement, "Average") ?? new XElement("Average"), "IOP_mmHg");
        if (values.Count == 0 && string.IsNullOrWhiteSpace(average))
        {
            return null;
        }

        var valuePart = values.Count == 0 ? string.Empty : string.Join(" ", values.Select(FormatPlainNumber));
        var averagePart = string.IsNullOrWhiteSpace(average) ? string.Empty : $"[{FormatOneDecimal(average)}]";
        return $"{label} = {string.Join(" ", new[] { valuePart, averagePart }.Where(part => !string.IsNullOrWhiteSpace(part)))}";
    }

    private static TopconTrk2PCorrectedIop GetTopconTrk2PCorrectedIopParts(XElement tmMeasure)
    {
        var correctedIop = FindChild(tmMeasure, "CorrectedIOP");
        var formula = correctedIop is null ? null : FindChild(correctedIop, "Formula1");
        if (formula is null)
        {
            return new TopconTrk2PCorrectedIop(
                new TopconTrk2PCorrectedIopParts(null, null, null, null, null),
                new TopconTrk2PCorrectedIopParts(null, null, null, null, null));
        }

        return new TopconTrk2PCorrectedIop(
            GetTopconTrk2PCorrectedEyeParts(FindChild(formula, "R")),
            GetTopconTrk2PCorrectedEyeParts(FindChild(formula, "L")));
    }

    private static TopconTrk2PCorrectedIopParts GetTopconTrk2PCorrectedEyeParts(XElement? eyeElement)
    {
        if (eyeElement is null)
        {
            return new TopconTrk2PCorrectedIopParts(null, null, null, null, null);
        }

        var measured = GetChildValue(FindChild(eyeElement, "Measured") ?? new XElement("Measured"), "IOP_mmHg");
        var corrected = GetChildValue(FindChild(eyeElement, "Corrected") ?? new XElement("Corrected"), "IOP_mmHg");

        return new TopconTrk2PCorrectedIopParts(
            measured,
            corrected,
            GetChildValue(eyeElement, "Param1"),
            GetChildValue(eyeElement, "Param2"),
            GetChildValue(eyeElement, "CCT"));
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
                "Measure[@Type='NT530P']/Pachy/HeaderLine",
                "NT530P MEDISTAR Pachymetrie-Überschrift",
                "Pachymetrie",
                null,
                null,
                "PACHY");
            AddMeasurement(
                measurements,
                "Measure[@Type='NT530P']/Pachy/MedistarLine",
                "NT530P MEDISTAR Pachymetrie-Zeile",
                pachyLine,
                null,
                null,
                "PACHY");
        }

        var tonoLines = BuildNt530PTonoMedistarLines(right, left, time);
        if (tonoLines.Count > 0)
        {
            AddMeasurement(
                measurements,
                "Measure[@Type='NT530P']/Tono/HeaderLine",
                "NT530P MEDISTAR Tonometrie-Überschrift",
                "Tonometrie",
                null,
                null,
                "NT");

            foreach (var line in tonoLines)
            {
                AddMeasurement(
                    measurements,
                    $"Measure[@Type='NT530P']/Tono/{line.Key}",
                    line.DisplayName,
                    line.Value,
                    null,
                    null,
                    "NT");
            }

            AddMeasurement(
                measurements,
                "Measure[@Type='NT530P']/Tono/MedistarLine",
                "NT530P MEDISTAR Tonometrie-Zeile",
                string.Join(" ", tonoLines.Select(line => line.Value)),
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

    private static IReadOnlyList<Nt530PMedistarLine> BuildNt530PTonoMedistarLines(XElement? right, XElement? left, string? time)
    {
        var lines = new List<Nt530PMedistarLine>();

        var rightPachy = BuildNt530PPachyListSegment("PR", right);
        if (!string.IsNullOrWhiteSpace(rightPachy))
        {
            lines.Add(new Nt530PMedistarLine(
                "PachyRightLine",
                "NT530P MEDISTAR Tonometrie Pachymetrie rechts",
                rightPachy));
        }

        var leftPachy = BuildNt530PPachyListSegment("PL", left);
        if (!string.IsNullOrWhiteSpace(leftPachy))
        {
            lines.Add(new Nt530PMedistarLine(
                "PachyLeftLine",
                "NT530P MEDISTAR Tonometrie Pachymetrie links",
                leftPachy));
        }

        var rightCorrected = GetNt530PCorrectedIopParts(right);
        if (!string.IsNullOrWhiteSpace(rightCorrected.Measured))
        {
            lines.Add(new Nt530PMedistarLine(
                "MeasuredRightLine",
                "NT530P MEDISTAR Tonometrie Messung rechts",
                $"PR: Gemessen = {FormatOneDecimal(rightCorrected.Measured)} mmHg;"));
        }

        var rightCorrectedAndParameters = BuildNt530PRightCorrectedAndParameterLine(rightCorrected);
        if (!string.IsNullOrWhiteSpace(rightCorrectedAndParameters))
        {
            lines.Add(new Nt530PMedistarLine(
                "CorrectedRightLine",
                "NT530P MEDISTAR Tonometrie Korrektur rechts",
                rightCorrectedAndParameters));
        }

        var leftCorrected = GetNt530PCorrectedIopParts(left);
        var rightCctAndLeftMeasured = BuildNt530PRightCctAndLeftMeasuredLine(rightCorrected, leftCorrected);
        if (!string.IsNullOrWhiteSpace(rightCctAndLeftMeasured))
        {
            lines.Add(new Nt530PMedistarLine(
                "RightCctLeftMeasuredLine",
                "NT530P MEDISTAR Tonometrie CCT rechts und Messung links",
                rightCctAndLeftMeasured));
        }

        var leftParameters = BuildNt530PLeftParameterLine(leftCorrected);
        if (!string.IsNullOrWhiteSpace(leftParameters))
        {
            lines.Add(new Nt530PMedistarLine(
                "ParameterLeftLine",
                "NT530P MEDISTAR Tonometrie Parameter links",
                leftParameters));
        }

        var tonometryList = BuildNt530PTonometryListSegment(right, left);
        var leftCctAndTonometry = BuildNt530PLeftCctAndTonometryLine(leftCorrected, tonometryList, time);
        if (!string.IsNullOrWhiteSpace(leftCctAndTonometry))
        {
            lines.Add(new Nt530PMedistarLine(
                "TonoListLine",
                "NT530P MEDISTAR Tonometrie Einzelwerte",
                leftCctAndTonometry));
        }

        return lines;
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

    private static Nt530PCorrectedIopParts GetNt530PCorrectedIopParts(XElement? eyeElement)
    {
        var nt = eyeElement is null ? null : FindChild(eyeElement, "NT");
        var correctedIop = nt is null ? null : FindChild(nt, "CorrectedIOP");
        if (correctedIop is null)
        {
            return new Nt530PCorrectedIopParts(null, null, null, null, null);
        }

        var measured = GetChildValue(FindChild(correctedIop, "Measured") ?? new XElement("Measured"), "mmHg");
        var corrected = GetChildValue(FindChild(correctedIop, "Corrected") ?? new XElement("Corrected"), "mmHg");
        var param1 = GetChildValue(correctedIop, "Param1");
        var param2 = GetChildValue(correctedIop, "Param2");
        var cct = GetChildValue(correctedIop, "CCT");

        return new Nt530PCorrectedIopParts(measured, corrected, param1, param2, cct);
    }

    private static string? BuildNt530PRightCorrectedAndParameterLine(Nt530PCorrectedIopParts right)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(right.Corrected))
        {
            parts.Add($"Korrigiert = {FormatOneDecimal(right.Corrected)} mmHg");
        }

        var parameters = BuildNt530PParameterParts(right, includeCct: false);
        if (parameters.Count > 0)
        {
            parts.Add($"PR: {string.Join("; ", parameters)}");
        }

        return parts.Count == 0 ? null : string.Join(" Y  ", parts) + ";";
    }

    private static string? BuildNt530PRightCctAndLeftMeasuredLine(
        Nt530PCorrectedIopParts right,
        Nt530PCorrectedIopParts left)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(right.Cct))
        {
            parts.Add($"CCT = {right.Cct}");
        }

        var leftMeasurementParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(left.Measured))
        {
            leftMeasurementParts.Add($"Gemessen = {FormatOneDecimal(left.Measured)} mmHg");
        }

        if (!string.IsNullOrWhiteSpace(left.Corrected))
        {
            leftMeasurementParts.Add($"Korrigiert = {FormatOneDecimal(left.Corrected)} mmHg");
        }

        if (leftMeasurementParts.Count > 0)
        {
            parts.Add($"PL: {string.Join("; ", leftMeasurementParts)}");
        }

        if (parts.Count == 0)
        {
            return null;
        }

        var line = string.Join(" Y  ", parts);
        return BuildNt530PParameterParts(left, includeCct: false).Count > 0 ? $"{line} Y" : line;
    }

    private static string? BuildNt530PLeftParameterLine(Nt530PCorrectedIopParts left)
    {
        var parameters = BuildNt530PParameterParts(left, includeCct: false);
        return parameters.Count == 0 ? null : $"PL: {string.Join("; ", parameters)};";
    }

    private static string? BuildNt530PLeftCctAndTonometryLine(
        Nt530PCorrectedIopParts left,
        string? tonometryList,
        string? time)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(left.Cct))
        {
            parts.Add($"CCT = {left.Cct}");
        }

        if (!string.IsNullOrWhiteSpace(tonometryList))
        {
            parts.Add($"P  {tonometryList}");
        }

        var formattedTime = FormatNt530PTime(time);
        if (!string.IsNullOrWhiteSpace(formattedTime))
        {
            parts.Add(formattedTime);
        }

        return parts.Count == 0 ? null : string.Join(" ", parts);
    }

    private static List<string> BuildNt530PParameterParts(Nt530PCorrectedIopParts parts, bool includeCct)
    {
        return new[]
            {
                string.IsNullOrWhiteSpace(parts.Param1) ? null : $"Param1 = {parts.Param1}",
                string.IsNullOrWhiteSpace(parts.Param2) ? null : $"Param2 = {parts.Param2}",
                !includeCct || string.IsNullOrWhiteSpace(parts.Cct) ? null : $"CCT = {parts.Cct}"
            }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!)
            .ToList();
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

    private static string? BuildRefractionEyeLine(string eye, XElement? values)
    {
        if (values is null)
        {
            return null;
        }

        var sphere = GetChildValue(values, "Sphere", "Sph", "Sphare");
        var cylinder = GetChildValue(values, "Cylinder", "Cyl");
        var axis = GetChildValue(values, "Axis");
        if (string.IsNullOrWhiteSpace(sphere)
            || string.IsNullOrWhiteSpace(cylinder)
            || string.IsNullOrWhiteSpace(axis))
        {
            return null;
        }

        return $"{eye}.:S={FormatSignedDecimal(sphere)} Z={FormatSignedDecimal(cylinder)}*{FormatAxis(axis)}";
    }

    private static string? BuildKeratometryRadiiSegment(string eye, XElement? median)
    {
        if (median is null)
        {
            return null;
        }

        var r1 = BuildKeratometryRadiusPart("R1", FindChild(median, "R1"));
        var r2 = BuildKeratometryRadiusPart("R2", FindChild(median, "R2"));
        if (string.IsNullOrWhiteSpace(r1) || string.IsNullOrWhiteSpace(r2))
        {
            return null;
        }

        return $"{eye}: {r1} {r2}";
    }

    private static string? BuildKeratometryRadiusPart(string label, XElement? element)
    {
        if (element is null)
        {
            return null;
        }

        var radius = GetChildValue(element, "Radius");
        var power = GetChildValue(element, "Power");
        var axis = GetChildValue(element, "Axis");
        if (string.IsNullOrWhiteSpace(radius)
            || string.IsNullOrWhiteSpace(power)
            || string.IsNullOrWhiteSpace(axis))
        {
            return null;
        }

        return $"{label}={FormatTwoDecimal(radius)} {FormatTwoDecimal(power)} *{FormatAxisCompact(axis)}";
    }

    private static string? BuildKeratometryAverageSegment(string eye, XElement? median)
    {
        if (median is null)
        {
            return null;
        }

        var average = FindChild(median, "Average");
        var cylinder = FindChild(median, "Cylinder");
        if (average is null || cylinder is null)
        {
            return null;
        }

        var averageRadius = GetChildValue(average, "Radius");
        var averagePower = GetChildValue(average, "Power");
        var cylinderPower = GetChildValue(cylinder, "Power");
        var cylinderAxis = GetChildValue(cylinder, "Axis");
        if (string.IsNullOrWhiteSpace(averageRadius)
            || string.IsNullOrWhiteSpace(averagePower)
            || string.IsNullOrWhiteSpace(cylinderPower)
            || string.IsNullOrWhiteSpace(cylinderAxis))
        {
            return null;
        }

        return $"{eye}: AV={FormatTwoDecimal(averageRadius)} {FormatTwoDecimal(averagePower)} CYL={FormatSignedCompactDecimal(cylinderPower)} {FormatAxisCompact(cylinderAxis)}";
    }

    private static IReadOnlyList<string> BuildSubjectiveRefractionLines(string typeName, XElement examDistance)
    {
        var refractionData = FindChild(examDistance, "RefractionData");
        if (refractionData is null)
        {
            return Array.Empty<string>();
        }

        var va = FindChild(examDistance, "VA");
        var right = BuildSubjectiveEyeSegment("R", FindChild(refractionData, "R"), va);
        var left = BuildSubjectiveEyeSegment("L", FindChild(refractionData, "L"), va);
        var eyeSegments = new[] { right, left }
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .Select(segment => segment!)
            .ToArray();
        if (eyeSegments.Length == 0)
        {
            return Array.Empty<string>();
        }

        var headerLine = $"Subjektive Refraktion {NormalizeSubjectiveTypeName(typeName)} {DetermineDistanceLabel(GetChildValue(examDistance, "Distance"))}:";
        var valueParts = new List<string> { string.Join(" / ", eyeSegments) };

        var pd = GetChildValue(FindChild(examDistance, "PD") ?? new XElement("PD"), "B");
        if (!string.IsNullOrWhiteSpace(pd))
        {
            valueParts.Add($"PD={FormatPd(pd)}");
        }

        var vd = GetChildValue(examDistance, "VD") ?? GetChildValue(refractionData, "VD");
        if (!string.IsNullOrWhiteSpace(vd))
        {
            valueParts.Add($"VD={FormatTwoDecimal(vd)}");
        }

        return new[] { headerLine, string.Join(" ", valueParts) };
    }

    private static string? BuildSubjectiveEyeSegment(string eye, XElement? eyeElement, XElement? va)
    {
        var line = BuildRefractionEyeLine(eye, eyeElement);
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        var visualAcuity = va is null ? null : GetChildValue(va, eye);
        return string.IsNullOrWhiteSpace(visualAcuity)
            ? line
            : $"{line} VA={visualAcuity.Trim()}";
    }

    private static string? BuildCv5000EyeLine(string eye, XElement? eyeElement, string? binocularPd, string? vd)
    {
        var line = BuildRefractionEyeLine(eye, eyeElement);
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(binocularPd))
        {
            line += $" PD= {FormatPd(binocularPd)}";
        }

        if (!string.IsNullOrWhiteSpace(vd))
        {
            line += $" VD= {FormatTwoDecimal(vd)}";
        }

        return line;
    }

    private static string? GetCv5000BinocularPd(XElement? pdElement)
    {
        if (pdElement is null)
        {
            return null;
        }

        var binocularPd = GetChildValue(pdElement, "B");
        if (!string.IsNullOrWhiteSpace(binocularPd))
        {
            return binocularPd;
        }

        var rightPd = GetChildValue(pdElement, "R");
        var leftPd = GetChildValue(pdElement, "L");
        return TryParseDecimal(rightPd ?? string.Empty, out var right)
            && TryParseDecimal(leftPd ?? string.Empty, out var left)
            ? (right + left).ToString("0.00", CultureInfo.InvariantCulture)
            : null;
    }

    private static void AddIfNotEmpty(List<string> lines, string? line)
    {
        if (!string.IsNullOrWhiteSpace(line))
        {
            lines.Add(line);
        }
    }

    private static string NormalizeSubjectiveTypeName(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "Unbenannt"
            : string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool IsCv5000PrescriptionType(string? value)
    {
        return string.Equals(
            NormalizeSubjectiveTypeName(value ?? string.Empty),
            "Prescription",
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCv5000FullCorrectionType(string? value)
    {
        return string.Equals(
            NormalizeSubjectiveTypeName(value ?? string.Empty),
            "Full Correction",
            StringComparison.OrdinalIgnoreCase);
    }

    private static string DetermineDistanceLabel(string? distance)
    {
        if (!TryParseDecimal(distance ?? string.Empty, out var value))
        {
            return string.Empty;
        }

        return value <= 100m ? "NEAR" : "FAR";
    }

    private static string FormatMicrometersAsMillimeters(string value)
    {
        if (!TryParseDecimal(value, out var micrometers))
        {
            return value.Trim();
        }

        return (micrometers / 1000m).ToString("0.000", CultureInfo.InvariantCulture);
    }

    private static string FormatMillimetersAsMicrometers(string value)
    {
        if (!TryParseDecimal(value, out var millimeters))
        {
            return value.Trim();
        }

        return (millimeters * 1000m).ToString("0", CultureInfo.InvariantCulture);
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

    private static string FormatTwoDecimal(string value)
    {
        if (!TryParseDecimal(value, out var number))
        {
            return value.Trim();
        }

        return number.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string FormatThreeDecimal(string value)
    {
        if (!TryParseDecimal(value, out var number))
        {
            return value.Trim();
        }

        return number.ToString("0.000", CultureInfo.InvariantCulture);
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

    private static bool IsTopconCl300Model(string? modelName)
    {
        var normalized = modelName?.Trim().Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
        return string.Equals(normalized, "CL300", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTopconKr800SModel(string? modelName)
    {
        var normalized = modelName?.Trim().Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
        return string.Equals(normalized, "KR800S", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTopconTrk2PModel(string? modelName)
    {
        var normalized = modelName?.Trim().Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
        return string.Equals(normalized, "TRK2P", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTopconCt1PModel(string? modelName)
    {
        var normalized = modelName?.Trim().Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
        return string.Equals(normalized, "CT1P", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTopconCv5000Model(string? modelName)
    {
        var normalized = modelName?.Trim().Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
        return string.Equals(normalized, "CV5000", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "CV5000S", StringComparison.OrdinalIgnoreCase);
    }

    private static void AddLensmeterLine(
        List<MeasurementValue> measurements,
        string eye,
        XElement? eyeElement,
        string? binocularPd,
        bool includeSignedPrismComponents = false)
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
        line = AppendAddition(line, "A", GetChildValue(eyeElement, "ADD", "Add1"));
        line = AppendAddition(line, "A2", GetChildValue(eyeElement, "ADD2", "Add2"));
        line = AppendPrism(line, eyeElement);
        if (includeSignedPrismComponents)
        {
            line = AppendSignedPrismComponents(line, eyeElement);
        }

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

    private static string AppendSignedPrismComponents(string line, XElement eyeElement)
    {
        var parts = new List<string>();
        AddSignedPrismComponent(parts, "H", GetChildValue(eyeElement, "H"));
        AddSignedPrismComponent(parts, "V", GetChildValue(eyeElement, "V"));

        return parts.Count == 0
            ? line
            : $"{line} P={string.Join(" ", parts)}";
    }

    private static void AddSignedPrismComponent(List<string> parts, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !TryParseDecimal(value, out var number)
            || number == 0m)
        {
            return;
        }

        parts.Add($"{label}={FormatSignedCompactDecimal(number)}");
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

    private static string FormatSignedCompactDecimal(decimal number)
    {
        var sign = number < 0 ? "-" : "+";
        return sign + Math.Abs(number).ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string FormatSignedCompactDecimal(string value)
    {
        if (!TryParseDecimal(value, out var number))
        {
            return value.Trim();
        }

        return FormatSignedCompactDecimal(number);
    }

    private static string FormatAxis(string value)
    {
        if (decimal.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var axis))
        {
            return ((int)Math.Round(axis, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture).PadLeft(3);
        }

        return value.Trim().PadLeft(3);
    }

    private static string FormatAxisCompact(string value)
    {
        if (decimal.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var axis))
        {
            return ((int)Math.Round(axis, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);
        }

        return value.Trim();
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

    private sealed record Nt530PMedistarLine(string Key, string DisplayName, string Value);

    private sealed record Nt530PCorrectedIopParts(
        string? Measured,
        string? Corrected,
        string? Param1,
        string? Param2,
        string? Cct);

    private sealed record TopconTrk2PMedistarLine(string Key, string DisplayName, string Value);

    private sealed record TopconTrk2PCctValues(string? SelectedMillimeters, IReadOnlyList<string> ListMillimeters)
    {
        public static TopconTrk2PCctValues Empty { get; } = new(null, Array.Empty<string>());
    }

    private sealed record TopconTrk2PCorrectedIop(TopconTrk2PCorrectedIopParts Right, TopconTrk2PCorrectedIopParts Left);

    private sealed record TopconTrk2PCorrectedIopParts(
        string? Measured,
        string? Corrected,
        string? Param1,
        string? Param2,
        string? Cct);
}
