namespace XdtDeviceBridge.Core;

public sealed class TomeyEmDeviceParser
{
    public const string ParserMode = "TomeyEm";

    public static bool IsParserMode(string? parserMode)
    {
        return string.Equals(parserMode, ParserMode, StringComparison.OrdinalIgnoreCase);
    }

    public DeviceParseResult ParseFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return ParseText(File.ReadAllText(path), path);
    }

    public DeviceParseResult ParseText(string rawText, string? sourcePath = null)
    {
        var text = rawText ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return CreateError("TOMEY-EM-Rohdaten sind leer.", sourcePath);
        }

        if (!LooksLikeEmCsv(text))
        {
            return CreateError("TOMEY-EM-Rohdaten konnten keinem unterstützten EM-CSV-Format zugeordnet werden.", sourcePath);
        }

        var model = DetectModel(text, sourcePath);
        var measurements = CreateCommonMeasurements(model);
        var issues = new List<DeviceParseIssue>();
        var tokens = ReadTokenLines(text);

        if (string.Equals(model, "EM-4000", StringComparison.OrdinalIgnoreCase))
        {
            ParseEm4000(tokens, measurements);
        }
        else
        {
            ParseEm3000(tokens, measurements);
        }

        AddPatientId(tokens, measurements);
        AddComments(tokens, measurements);
        AddAttachments(tokens, measurements, model);
        AddNoExportableWarningIfNeeded(measurements, issues, $"TOMEY {model}-CSV wurde gelesen, aber keine exportierbaren Endothel-Messwerte, Kommentare oder Bildverweise erkannt.");
        return new DeviceParseResult(measurements, issues);
    }

    private static void ParseEm3000(IReadOnlyList<TokenLine> tokens, List<MeasurementValue> measurements)
    {
        var eyes = new Dictionary<string, Em3000EyeValues>(StringComparer.OrdinalIgnoreCase);
        string? currentEye = null;

        foreach (var line in tokens)
        {
            if (IsToken(line, "[RL]"))
            {
                currentEye = NormalizeEye(line.Values.ElementAtOrDefault(0));
                if (!string.IsNullOrWhiteSpace(currentEye))
                {
                    eyes[currentEye] = GetEm3000Eye(eyes, currentEye) with { Eye = currentEye };
                }

                continue;
            }

            if (string.IsNullOrWhiteSpace(currentEye))
            {
                continue;
            }

            if (IsToken(line, "[NUMBER]"))
            {
                eyes[currentEye] = GetEm3000Eye(eyes, currentEye) with { Eye = currentEye, Number = ReadFirstValue(line) };
            }
            else if (IsToken(line, "[DENSITY]"))
            {
                eyes[currentEye] = GetEm3000Eye(eyes, currentEye) with { Eye = currentEye, Density = ReadFirstValue(line) };
            }
            else if (IsToken(line, "[THK]"))
            {
                eyes[currentEye] = GetEm3000Eye(eyes, currentEye) with { Eye = currentEye, Thickness = ReadFirstValue(line) };
            }
        }

        foreach (var eye in new[] { "R", "L" })
        {
            if (!eyes.TryGetValue(eye, out var values))
            {
                continue;
            }

            AddOptional(measurements, $"Measure[@Type='EM']/Endothelium/{eye}/Number", $"Endothel {eye} Anzahl", values.Number, null, eye, "EM");
            AddOptional(measurements, $"Measure[@Type='EM']/Endothelium/{eye}/Density", $"Endothel {eye} Dichte", values.Density, "mm2", eye, "EM");
            AddOptional(measurements, $"Measure[@Type='EM']/Endothelium/{eye}/Thickness", $"Endothel {eye} Hornhautdicke", values.Thickness, "um", eye, "EM");
            AddOptional(measurements, $"Measure[@Type='EM']/Endothelium/{eye}/MedistarLine", $"Endothel {eye} MEDISTAR-Zeile", BuildEm3000Line(eye, values), null, eye, "EM");
        }
    }

    private static void ParseEm4000(IReadOnlyList<TokenLine> tokens, List<MeasurementValue> measurements)
    {
        var right = new Em4000EyeValues("R");
        var left = new Em4000EyeValues("L");

        foreach (var line in tokens)
        {
            if (IsToken(line, "[DENSITY_1]"))
            {
                right = right with { CellDensity = ReadFirstValue(line) };
            }
            else if (IsToken(line, "[THK_1]"))
            {
                right = right with { Cct = ReadFirstValue(line) };
            }
            else if (IsToken(line, "[DENSITY_2]"))
            {
                left = left with { CellDensity = ReadFirstValue(line) };
            }
            else if (IsToken(line, "[THK_2]"))
            {
                left = left with { Cct = ReadFirstValue(line) };
            }
        }

        AddEm4000Eye(measurements, right);
        AddEm4000Eye(measurements, left);
    }

    private static void AddEm4000Eye(List<MeasurementValue> measurements, Em4000EyeValues values)
    {
        var eye = values.Eye;
        AddOptional(measurements, $"Measure[@Type='EM']/Endothelium/{eye}/CellDensity", $"Endothel {eye} CD", values.CellDensity, null, eye, "EM");
        AddOptional(measurements, $"Measure[@Type='EM']/Endothelium/{eye}/CCT", $"Endothel {eye} CCT", values.Cct, null, eye, "EM");
        AddOptional(measurements, $"Measure[@Type='EM']/Endothelium/{eye}/CellDensity/MedistarLine", $"Endothel {eye} CD MEDISTAR-Zeile", BuildSingleMetricLine(eye, "CD", values.CellDensity), null, eye, "EM");
        AddOptional(measurements, $"Measure[@Type='EM']/Endothelium/{eye}/CCT/MedistarLine", $"Endothel {eye} CCT MEDISTAR-Zeile", BuildSingleMetricLine(eye, "CCT", values.Cct), null, eye, "EM");
    }

    private static void AddPatientId(IReadOnlyList<TokenLine> tokens, List<MeasurementValue> measurements)
    {
        var patientId = tokens
            .FirstOrDefault(line => IsToken(line, "[PT_ID]"))
            ?.Values
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        AddOptional(measurements, "Patient/Id", "Patient ID aus Gerätedatei", patientId, null, null, "Patient");
    }

    private static void AddComments(IReadOnlyList<TokenLine> tokens, List<MeasurementValue> measurements)
    {
        var comments = tokens
            .Where(line =>
                IsToken(line, "[COMMENT]")
                || IsToken(line, "[COMMENT_R]")
                || IsToken(line, "[COMMENT_L]")
                || IsToken(line, "[NOTE]")
                || IsToken(line, "[REMARK]"))
            .Select(line => string.Join(" ", line.Values.Where(value => !string.IsNullOrWhiteSpace(value))))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
        if (comments.Length == 0)
        {
            return;
        }

        AddOptional(measurements, "Measure[@Type='EM']/Comment/MedistarLine", "Endothel Kommentar MEDISTAR-Zeile", string.Join(" // ", comments), null, null, "EM");
    }

    private static void AddAttachments(IReadOnlyList<TokenLine> tokens, List<MeasurementValue> measurements, string model)
    {
        var imageIndex = 1;
        foreach (var line in tokens.Where(line => IsToken(line, "[FILE]")))
        {
            var path = line.Values.ElementAtOrDefault(0);
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            var normalizedPath = path.Trim();
            var description = CreateAttachmentDescription(line, model);
            var format = Path.GetExtension(normalizedPath).TrimStart('.').ToUpperInvariant();
            var fileName = Path.GetFileName(normalizedPath);
            var basePath = $"Measure[@Type='EM']/Attachment/Image{imageIndex}";

            AddOptional(measurements, $"{basePath}/Path", $"Endothel Bild {imageIndex} Pfad", normalizedPath, null, null, "EM");
            AddOptional(measurements, $"{basePath}/FileName", $"Endothel Bild {imageIndex} Datei", fileName, null, null, "EM");
            AddOptional(measurements, $"{basePath}/Format", $"Endothel Bild {imageIndex} Format", format, null, null, "EM");
            AddOptional(measurements, $"{basePath}/Description", $"Endothel Bild {imageIndex} Beschreibung", description, null, null, "EM");
            AddOptional(measurements, $"{basePath}/MedistarLine", $"Endothel Bild {imageIndex} Verweis", normalizedPath, null, null, "EM");
            imageIndex++;
        }
    }

    private static string CreateAttachmentDescription(TokenLine line, string model)
    {
        if (string.Equals(model, "EM-4000", StringComparison.OrdinalIgnoreCase))
        {
            var eye = NormalizeEye(line.Values.ElementAtOrDefault(2));
            return string.IsNullOrWhiteSpace(eye) ? "Bild" : $"Bild {eye}";
        }

        return line.Values.ElementAtOrDefault(1)?.Trim() is { Length: > 0 } description
            ? description
            : "Bild";
    }

    private static string? BuildEm3000Line(string eye, Em3000EyeValues values)
    {
        if (string.IsNullOrWhiteSpace(values.Number)
            || string.IsNullOrWhiteSpace(values.Density)
            || string.IsNullOrWhiteSpace(values.Thickness))
        {
            return null;
        }

        return $"{eye}: Anzahl = {values.Number}; Dichte = {values.Density} mm2; Hornhautdicke = {values.Thickness} um";
    }

    private static string? BuildSingleMetricLine(string eye, string name, string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : $"{eye} {name} = {value}";
    }

    private static IReadOnlyList<TokenLine> ReadTokenLines(string rawText)
    {
        var result = new List<TokenLine>();
        foreach (var rawLine in rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.Trim().TrimStart('\uFEFF');
            if (line.Length == 0)
            {
                continue;
            }

            var parts = line.Split(',').Select(part => part.Trim()).ToArray();
            if (parts.Length > 0 && parts[0].Length > 0)
            {
                result.Add(new TokenLine(parts[0], parts.Skip(1).ToArray()));
            }
        }

        return result;
    }

    private static bool LooksLikeEmCsv(string rawText)
    {
        return rawText.Contains("[RL_1]", StringComparison.OrdinalIgnoreCase)
            || rawText.Contains("[DENSITY_1]", StringComparison.OrdinalIgnoreCase)
            || rawText.Contains("[THK_1]", StringComparison.OrdinalIgnoreCase)
            || (rawText.Contains("[RL]", StringComparison.OrdinalIgnoreCase)
                && (rawText.Contains("[NUMBER]", StringComparison.OrdinalIgnoreCase)
                    || rawText.Contains("[DENSITY]", StringComparison.OrdinalIgnoreCase)
                    || rawText.Contains("[THK]", StringComparison.OrdinalIgnoreCase)));
    }

    private static string DetectModel(string rawText, string? sourcePath)
    {
        var fileName = Path.GetFileName(sourcePath ?? string.Empty);
        if (fileName.Contains("EM-4000", StringComparison.OrdinalIgnoreCase)
            || fileName.Contains("EM4000", StringComparison.OrdinalIgnoreCase)
            || rawText.Contains("[RL_1]", StringComparison.OrdinalIgnoreCase)
            || rawText.Contains("[DENSITY_1]", StringComparison.OrdinalIgnoreCase))
        {
            return "EM-4000";
        }

        return "EM-3000";
    }

    private static string? NormalizeEye(string? value)
    {
        var normalized = value?.Trim().TrimStart('[').TrimEnd(']') ?? string.Empty;
        return normalized.Equals("Right", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("R", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("r", StringComparison.Ordinal)
                ? "R"
                : normalized.Equals("Left", StringComparison.OrdinalIgnoreCase)
                    || normalized.Equals("L", StringComparison.OrdinalIgnoreCase)
                    || normalized.Equals("l", StringComparison.Ordinal)
                        ? "L"
                        : null;
    }

    private static string? ReadFirstValue(TokenLine line)
    {
        return line.Values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
    }

    private static bool IsToken(TokenLine line, string token)
    {
        return string.Equals(line.Token, token, StringComparison.OrdinalIgnoreCase);
    }

    private static List<MeasurementValue> CreateCommonMeasurements(string model)
    {
        return new List<MeasurementValue>
        {
            new("Common/Company", "Company", "TOMEY", null, null, "Common"),
            new("Common/ModelName", "ModelName", model, null, null, "Common")
        };
    }

    private static void AddNoExportableWarningIfNeeded(
        IReadOnlyCollection<MeasurementValue> measurements,
        List<DeviceParseIssue> issues,
        string message)
    {
        if (measurements.Any(measurement => !measurement.SourcePath.StartsWith("Common/", StringComparison.OrdinalIgnoreCase)
            && !measurement.SourcePath.Equals("Patient/Id", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        issues.Add(new DeviceParseIssue(DeviceParseIssueSeverity.Warning, message, string.Empty, null));
    }

    private static void AddOptional(
        List<MeasurementValue> measurements,
        string sourcePath,
        string displayName,
        string? value,
        string? unit,
        string? eye,
        string group)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        measurements.Add(new MeasurementValue(sourcePath, displayName, value.Trim(), unit, eye, group));
    }

    private static DeviceParseResult CreateError(string message, string? sourcePath)
    {
        return new DeviceParseResult(
            Array.Empty<MeasurementValue>(),
            new[] { new DeviceParseIssue(DeviceParseIssueSeverity.Error, message, sourcePath ?? string.Empty, null) });
    }

    private static Em3000EyeValues GetEm3000Eye(Dictionary<string, Em3000EyeValues> eyes, string eye)
    {
        return eyes.TryGetValue(eye, out var values) ? values : new Em3000EyeValues(eye);
    }

    private sealed record TokenLine(string Token, string[] Values);

    private sealed record Em3000EyeValues(
        string Eye = "",
        string? Number = null,
        string? Density = null,
        string? Thickness = null);

    private sealed record Em4000EyeValues(
        string Eye,
        string? CellDensity = null,
        string? Cct = null);
}
