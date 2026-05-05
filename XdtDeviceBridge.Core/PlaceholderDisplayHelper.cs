namespace XdtDeviceBridge.Core;

public static class PlaceholderDisplayHelper
{
    public static string GetDisplayName(string placeholder, string? displayName = null)
    {
        return placeholder switch
        {
            "AIS.PatientNumber" => "Patientennummer",
            "AIS.LastName" => "Nachname",
            "AIS.FirstName" => "Vorname",
            "AIS.BirthDate" => "Geburtsdatum",
            "AIS.ExaminationType" => "Untersuchungsart",
            "AIS.Street" => "Straße",
            "AIS.PostalCodeCity" => "PLZ / Ort",
            _ => GetDeviceDisplayName(placeholder, displayName)
        };
    }

    public static string? GetSuggestedFormat(string placeholder)
    {
        if (!placeholder.StartsWith("Device.", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var sourcePath = StripDevicePrefix(placeholder);
        if (IsRawText(sourcePath))
        {
            return "Raw";
        }

        if (IsTime(sourcePath))
        {
            return "Time";
        }

        if (IsAxis(sourcePath))
        {
            return "Axis";
        }

        if (IsDiopter(sourcePath))
        {
            return "Diopter";
        }

        if (ContainsAny(sourcePath, "FarPD", "NearPD", "DistancePD", "PD"))
        {
            return "Pd";
        }

        if (ContainsAny(sourcePath, "Pachy", "PACHY", "CCT", "CornealThickness"))
        {
            return "Pachy";
        }

        if (ContainsAny(sourcePath, "Prism", "PrismX", "PrismY"))
        {
            return "Prism";
        }

        if (IsKeratometry(sourcePath))
        {
            return "Keratometry";
        }

        if (ContainsAny(sourcePath, "IOP", "Pressure", "Tension", "CorrectedIOP")
            || HasToken(sourcePath, "NT")
            || HasToken(sourcePath, "TM"))
        {
            return "Iop";
        }

        return null;
    }

    public static int GetDeviceSortOrder(string sourcePath)
    {
        var normalized = StripDevicePrefix(sourcePath);
        return (GetGroupSortOrder(normalized) * 100_000)
            + (GetEyeSortOrder(normalized) * 10_000)
            + (GetAggregationSortOrder(normalized) * 1_000)
            + GetMeasurementTypeSortOrder(normalized);
    }

    private static string GetDeviceDisplayName(string placeholder, string? displayName)
    {
        var sourcePath = StripDevicePrefix(placeholder);
        var name = GetMeasurementName(sourcePath, displayName);
        var eye = GetFriendlyEyeName(sourcePath);
        var context = GetMeasurementContext(sourcePath, name);

        var parts = new List<string> { name };
        if (!string.IsNullOrWhiteSpace(eye) && !NameAlreadyContainsEye(name))
        {
            parts.Add(eye);
        }

        if (!string.IsNullOrWhiteSpace(context))
        {
            parts.Add(context);
        }

        return string.Join(" ", parts);
    }

    private static string GetMeasurementName(string sourcePath, string? displayName)
    {
        if (ContainsAny(sourcePath, "DateTime"))
        {
            return "Datum/Uhrzeit";
        }

        if (IsLastSegment(sourcePath, "Date"))
        {
            return "Datum";
        }

        if (IsTime(sourcePath))
        {
            return "Uhrzeit";
        }

        if (IsLastSegment(sourcePath, "Company"))
        {
            return "Hersteller";
        }

        if (IsLastSegment(sourcePath, "Model") || IsLastSegment(sourcePath, "ModelName"))
        {
            return "Modell";
        }

        if (IsLastSegment(sourcePath, "Comment"))
        {
            return "Kommentar";
        }

        if (IsLastSegment(sourcePath, "CylinderMode"))
        {
            return "Zylindermodus";
        }

        if (IsLastSegment(sourcePath, "VD"))
        {
            return "Vertex Distance";
        }

        if (IsLastSegment(sourcePath, "WorkingDistance"))
        {
            return "Arbeitsabstand";
        }

        if (ContainsAny(sourcePath, "PACHYImage"))
        {
            return "Pachymetrie-Bild";
        }

        if (ContainsAny(sourcePath, "CorrectedIOP"))
        {
            return "korrigierter Augendruck";
        }

        if (IsPrismBase(sourcePath))
        {
            if (ContainsAny(sourcePath, "PrismX"))
            {
                return "Basisrichtung Prisma horizontal";
            }

            if (ContainsAny(sourcePath, "PrismY"))
            {
                return "Basisrichtung Prisma vertikal";
            }

            return "Basisrichtung";
        }

        if (ContainsAny(sourcePath, "PrismX"))
        {
            return "Prisma horizontal";
        }

        if (ContainsAny(sourcePath, "PrismY"))
        {
            return "Prisma vertikal";
        }

        if (ContainsAny(sourcePath, "Prism"))
        {
            return "Prisma";
        }

        if (ContainsAny(sourcePath, "FarPD", "DistancePD"))
        {
            return "Pupillendistanz Ferne";
        }

        if (ContainsAny(sourcePath, "NearPD"))
        {
            return "Pupillendistanz Nähe";
        }

        if (ContainsAny(sourcePath, "PD"))
        {
            return "Pupillendistanz";
        }

        if (IsKeratometry(sourcePath))
        {
            return GetKeratometryName(sourcePath);
        }

        if (ContainsAny(sourcePath, "IOP", "Pressure", "Tension")
            || (HasToken(sourcePath, "TM") && ContainsAny(sourcePath, "Avg", "Average", "Mean"))
            || (HasToken(sourcePath, "NT") && ContainsAny(sourcePath, "Avg", "Average", "Mean")))
        {
            return "Augendruck";
        }

        if (ContainsAny(sourcePath, "CCT"))
        {
            return "zentrale Hornhautdicke";
        }

        if (ContainsAny(sourcePath, "CornealThickness"))
        {
            return "Hornhautdicke";
        }

        if (ContainsAny(sourcePath, "Pachy", "PACHY") || HasToken(sourcePath, "PR") || HasToken(sourcePath, "PL"))
        {
            return "Pachymetrie";
        }

        if (ContainsAny(sourcePath, "BCVA"))
        {
            return "bestkorrigierter Visus";
        }

        if (ContainsAny(sourcePath, "VA", "Visus"))
        {
            return "Visus";
        }

        if (ContainsAny(sourcePath, "Add", "Addition"))
        {
            return "Addition";
        }

        if (HasToken(sourcePath, "SE"))
        {
            return "Sphärisches Äquivalent";
        }

        if (ContainsAny(sourcePath, "Sphere", "Sphare") || HasToken(sourcePath, "Sph"))
        {
            return "Sphäre";
        }

        if (ContainsAny(sourcePath, "Cylinder") || HasToken(sourcePath, "Cyl"))
        {
            return "Zylinder";
        }

        if (IsAxis(sourcePath))
        {
            return "Achse";
        }

        if (HasToken(sourcePath, "NT"))
        {
            return "Non-Contact-Tonometrie";
        }

        if (HasToken(sourcePath, "TM"))
        {
            return "Tonometrie";
        }

        if (HasToken(sourcePath, "LM"))
        {
            return "Lensmeter";
        }

        return !string.IsNullOrWhiteSpace(displayName)
            ? displayName.Trim()
            : DeriveNameFromSourcePath(sourcePath);
    }

    private static string GetKeratometryName(string sourcePath)
    {
        var radiusLabel = GetKeratometryRadiusLabel(sourcePath);
        if (IsAxis(sourcePath) && radiusLabel is not null)
        {
            return $"Achse Hornhautradius {radiusLabel}";
        }

        if (ContainsAny(sourcePath, "Power") && radiusLabel is not null)
        {
            return $"Keratometrie Brechkraft {radiusLabel}";
        }

        if (ContainsAny(sourcePath, "Radius") && radiusLabel is not null)
        {
            return $"Hornhautradius {radiusLabel}";
        }

        if (ContainsAny(sourcePath, "ra"))
        {
            return "errechneter Zylinder";
        }

        if (ContainsAny(sourcePath, "AV", "Average"))
        {
            return "Keratometrie Durchschnitt";
        }

        if (ContainsAny(sourcePath, "Power"))
        {
            return "Keratometrie Brechkraft";
        }

        if (ContainsAny(sourcePath, "Radius"))
        {
            return "Radius";
        }

        return radiusLabel is null ? "Keratometrie" : $"Hornhautradius {radiusLabel}";
    }

    private static string? GetKeratometryRadiusLabel(string sourcePath)
    {
        if (HasToken(sourcePath, "K1") || HasToken(sourcePath, "R1"))
        {
            return "R1";
        }

        if (HasToken(sourcePath, "K2") || HasToken(sourcePath, "R2"))
        {
            return "R2";
        }

        return null;
    }

    private static string GetMeasurementContext(string sourcePath, string name)
    {
        var measurementNumber = ExtractMeasurementNumber(sourcePath);
        if (ContainsAny(sourcePath, "ARMedian"))
        {
            return "Berechnung / Median";
        }

        if (ContainsAny(sourcePath, "ARList", "IOPList", "PachyList", "PACHYList")
            || (!string.IsNullOrWhiteSpace(measurementNumber) && ContainsAny(sourcePath, "List")))
        {
            return string.IsNullOrWhiteSpace(measurementNumber) ? "Messung" : $"Messung {measurementNumber}";
        }

        if (ContainsAny(sourcePath, "Avg", "Average", "Mean") && !NameContainsAny(name, "Mittelwert", "Durchschnitt"))
        {
            return IsKeratometry(sourcePath) ? "Durchschnitt" : "Mittelwert";
        }

        if (ContainsAny(sourcePath, "Near"))
        {
            return "Nähe";
        }

        if (ContainsAny(sourcePath, "Far", "Distance") && !ContainsAny(sourcePath, "WorkingDistance"))
        {
            return "Ferne";
        }

        if (ContainsAny(sourcePath, "/SR/", ".SR.", "SR/") || HasToken(sourcePath, "SR"))
        {
            return "SR";
        }

        if (ContainsAny(sourcePath, "TrialLens"))
        {
            return "TrialLens";
        }

        if (ContainsAny(sourcePath, "ContactLens"))
        {
            return "ContactLens";
        }

        return string.Empty;
    }

    private static bool IsDiopter(string sourcePath)
    {
        return ContainsAny(sourcePath, "Sphere", "Sphare", "Cylinder", "Addition")
            || HasToken(sourcePath, "Sph")
            || HasToken(sourcePath, "Cyl")
            || HasToken(sourcePath, "SE")
            || HasToken(sourcePath, "Add")
            || (HasToken(sourcePath, "ra") && IsKeratometry(sourcePath));
    }

    private static bool IsAxis(string sourcePath)
    {
        return ContainsAny(sourcePath, "Axis", "Axe", "Achse");
    }

    private static bool IsKeratometry(string sourcePath)
    {
        return HasToken(sourcePath, "KM")
            || ContainsAny(sourcePath, "Keratometry")
            || HasToken(sourcePath, "K1")
            || HasToken(sourcePath, "K2")
            || HasToken(sourcePath, "R1")
            || HasToken(sourcePath, "R2")
            || HasToken(sourcePath, "AV")
            || HasToken(sourcePath, "ra")
            || ContainsAny(sourcePath, "Radius", "Power");
    }

    private static bool IsRawText(string sourcePath)
    {
        return IsLastSegment(sourcePath, "Company")
            || IsLastSegment(sourcePath, "Model")
            || IsLastSegment(sourcePath, "ModelName")
            || IsLastSegment(sourcePath, "Comment")
            || IsLastSegment(sourcePath, "CylinderMode")
            || IsPrismBase(sourcePath);
    }

    private static bool IsTime(string sourcePath)
    {
        return IsLastSegment(sourcePath, "Time")
            || IsLastSegment(sourcePath, "MeasurementTime")
            || IsLastSegment(sourcePath, "Uhrzeit");
    }

    private static bool IsPrismBase(string sourcePath)
    {
        return ContainsAny(sourcePath, "@base")
            || IsLastSegment(sourcePath, "base")
            || IsLastSegment(sourcePath, "Base");
    }

    private static int GetGroupSortOrder(string sourcePath)
    {
        if (IsMeta(sourcePath))
        {
            return 0;
        }

        if (IsRefractive(sourcePath))
        {
            return 1;
        }

        if (HasToken(sourcePath, "LM") || ContainsAny(sourcePath, "Prism"))
        {
            return 2;
        }

        if (ContainsAny(sourcePath, "Pachy", "PACHY", "CCT", "CornealThickness"))
        {
            return 4;
        }

        if (ContainsAny(sourcePath, "IOP", "Pressure", "Tension", "CorrectedIOP") || HasToken(sourcePath, "NT") || HasToken(sourcePath, "TM"))
        {
            return 3;
        }

        if (IsKeratometry(sourcePath))
        {
            return 5;
        }

        return 6;
    }

    private static bool IsMeta(string sourcePath)
    {
        return ContainsAny(sourcePath, "DateTime")
            || IsLastSegment(sourcePath, "Date")
            || IsTime(sourcePath)
            || IsLastSegment(sourcePath, "Company")
            || IsLastSegment(sourcePath, "Model")
            || IsLastSegment(sourcePath, "ModelName")
            || IsLastSegment(sourcePath, "Comment")
            || IsLastSegment(sourcePath, "CylinderMode")
            || IsLastSegment(sourcePath, "VD")
            || IsLastSegment(sourcePath, "WorkingDistance");
    }

    private static bool IsRefractive(string sourcePath)
    {
        return HasToken(sourcePath, "AR")
            || ContainsAny(sourcePath, "ARMedian", "ARList", "Sphere", "Sphare", "Cylinder", "Axis", "Axe", "BCVA", "Visus", "TrialLens", "ContactLens")
            || HasToken(sourcePath, "Sph")
            || HasToken(sourcePath, "Cyl")
            || HasToken(sourcePath, "SE")
            || HasToken(sourcePath, "VA")
            || HasToken(sourcePath, "Add")
            || HasToken(sourcePath, "Addition")
            || HasToken(sourcePath, "Near")
            || HasToken(sourcePath, "Far")
            || HasToken(sourcePath, "Distance");
    }

    private static int GetEyeSortOrder(string sourcePath)
    {
        return GetFriendlyEyeName(sourcePath) switch
        {
            "rechts" => 0,
            "links" => 1,
            _ => 2
        };
    }

    private static int GetAggregationSortOrder(string sourcePath)
    {
        if (ContainsAny(sourcePath, "ARMedian", "Avg", "Average", "Mean"))
        {
            return 0;
        }

        if (ContainsAny(sourcePath, "ARList") || ContainsAny(sourcePath, "List"))
        {
            var measurementNumber = ExtractMeasurementNumber(sourcePath);
            return int.TryParse(measurementNumber, out var number) ? number : 5;
        }

        return 1;
    }

    private static int GetMeasurementTypeSortOrder(string sourcePath)
    {
        if (ContainsAny(sourcePath, "Sphere", "Sphare") || HasToken(sourcePath, "Sph"))
        {
            return 0;
        }

        if (ContainsAny(sourcePath, "Cylinder") || HasToken(sourcePath, "Cyl"))
        {
            return 1;
        }

        if (IsAxis(sourcePath))
        {
            return 2;
        }

        if (HasToken(sourcePath, "SE"))
        {
            return 3;
        }

        if (ContainsAny(sourcePath, "FarPD", "DistancePD"))
        {
            return 4;
        }

        if (ContainsAny(sourcePath, "NearPD"))
        {
            return 5;
        }

        if (ContainsAny(sourcePath, "IOP", "Pressure", "Tension"))
        {
            return 6;
        }

        if (ContainsAny(sourcePath, "Pachy", "PACHY", "CCT", "CornealThickness"))
        {
            return 7;
        }

        if (IsKeratometry(sourcePath))
        {
            return 8;
        }

        return 50;
    }

    private static string GetFriendlyEyeName(string sourcePath)
    {
        var segments = GetSegments(sourcePath);
        if (segments.Any(segment => string.Equals(segment, "R", StringComparison.OrdinalIgnoreCase)
            || string.Equals(segment, "Right", StringComparison.OrdinalIgnoreCase)
            || string.Equals(segment, "OD", StringComparison.OrdinalIgnoreCase)
            || string.Equals(segment, "PR", StringComparison.OrdinalIgnoreCase)))
        {
            return "rechts";
        }

        if (segments.Any(segment => string.Equals(segment, "L", StringComparison.OrdinalIgnoreCase)
            || string.Equals(segment, "Left", StringComparison.OrdinalIgnoreCase)
            || string.Equals(segment, "OS", StringComparison.OrdinalIgnoreCase)
            || string.Equals(segment, "PL", StringComparison.OrdinalIgnoreCase)))
        {
            return "links";
        }

        return string.Empty;
    }

    private static string ExtractMeasurementNumber(string sourcePath)
    {
        const string marker = "[@No=";
        var markerIndex = sourcePath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return string.Empty;
        }

        var valueStart = markerIndex + marker.Length;
        while (valueStart < sourcePath.Length && (sourcePath[valueStart] == '\'' || sourcePath[valueStart] == '"'))
        {
            valueStart++;
        }

        var valueEnd = valueStart;
        while (valueEnd < sourcePath.Length && sourcePath[valueEnd] != '\'' && sourcePath[valueEnd] != '"' && sourcePath[valueEnd] != ']')
        {
            valueEnd++;
        }

        return valueEnd <= valueStart ? string.Empty : sourcePath[valueStart..valueEnd];
    }

    private static string StripDevicePrefix(string placeholder)
    {
        return placeholder.StartsWith("Device.", StringComparison.OrdinalIgnoreCase)
            ? placeholder[7..]
            : placeholder;
    }

    private static bool NameAlreadyContainsEye(string name)
    {
        return NameContainsAny(name, " rechts", " links");
    }

    private static bool NameContainsAny(string value, params string[] parts)
    {
        return parts.Any(part => value.Contains(part, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsAny(string value, params string[] parts)
    {
        return parts.Any(part => value.Contains(part, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasToken(string sourcePath, string token)
    {
        return GetSegments(sourcePath).Any(segment => string.Equals(segment, token, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsLastSegment(string sourcePath, string segment)
    {
        return string.Equals(GetSegments(sourcePath).LastOrDefault(), segment, StringComparison.OrdinalIgnoreCase);
    }

    private static string DeriveNameFromSourcePath(string sourcePath)
    {
        var lastSegment = GetSegments(sourcePath).LastOrDefault();
        return string.IsNullOrWhiteSpace(lastSegment) ? sourcePath : lastSegment;
    }

    private static IReadOnlyList<string> GetSegments(string sourcePath)
    {
        return sourcePath
            .Split(new[] { '/', '.', '[', ']', '@', '=', '\'', '"' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(segment => !string.Equals(segment, "No", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }
}
