using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class BuilderManualProcessingPreviewService
{
    private readonly GdtParser _gdtParser = new();
    private readonly PatientDataMapper _patientDataMapper = new();
    private readonly MedistarHistoricalMeasurementParser _medistarHistoricalMeasurementParser = new();
    private readonly XmlDeviceParser _xmlDeviceParser = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly XdtExportBuilder _xdtExportBuilder = new();

    public ProcessingPipelineResult BuildPreview(BuilderManualProcessingPreviewRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.ExportProfile);

        var issues = new List<ProcessingIssue>();
        var aisReadResult = ReadAisPatientData(
            request.InterfaceProfile,
            request.DeviceProfile,
            request.AisFilePath);
        issues.AddRange(aisReadResult.Issues);
        if (!aisReadResult.Success || aisReadResult.Patient is null)
        {
            AddFailureMessages(issues, ProcessingStage.GdtParsing, aisReadResult.FailureMessages);
            return new ProcessingPipelineResult(aisReadResult.Patient, [], [], string.Empty, issues);
        }

        var patient = aisReadResult.Patient;
        var deviceResult = ReadDeviceData(request.DeviceProfile, request.DeviceFilePath);
        issues.AddRange(deviceResult.Issues.Select(issue => new ProcessingIssue(
            issue.Severity == DeviceParseIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.DeviceParsing,
            issue.Message)));
        if (deviceResult.HasErrors)
        {
            return new ProcessingPipelineResult(patient, deviceResult.Measurements, [], string.Empty, issues);
        }

        var mappingRules = _mappingAdapter.Adapt(request.ExportProfile);
        var mappingResult = _mappingEngine.Map(patient, deviceResult.Measurements, mappingRules);
        issues.AddRange(mappingResult.Issues.Select(issue => new ProcessingIssue(
            issue.Severity == MappingIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.Mapping,
            issue.Message)));
        if (mappingResult.HasErrors)
        {
            return new ProcessingPipelineResult(patient, deviceResult.Measurements, mappingResult.Records, string.Empty, issues);
        }

        var exportResult = _xdtExportBuilder.Build(mappingResult.Records);
        issues.AddRange(exportResult.Issues.Select(issue => new ProcessingIssue(
            issue.Severity == XdtExportIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.Export,
            issue.Message)));

        return new ProcessingPipelineResult(
            patient,
            deviceResult.Measurements,
            mappingResult.Records,
            exportResult.Content,
            issues);
    }

    private DeviceParseResult ReadDeviceData(DeviceProfileDefinition? deviceProfile, string deviceFilePath)
    {
        if (string.IsNullOrWhiteSpace(deviceFilePath))
        {
            return new DeviceParseResult(
                Array.Empty<MeasurementValue>(),
                new[]
                {
                    new DeviceParseIssue(DeviceParseIssueSeverity.Error, "Gerätedateipfad fehlt.", string.Empty, null)
                });
        }

        if (!File.Exists(deviceFilePath))
        {
            return new DeviceParseResult(
                Array.Empty<MeasurementValue>(),
                new[]
                {
                    new DeviceParseIssue(DeviceParseIssueSeverity.Error, $"Gerätedatei nicht gefunden: {deviceFilePath}", deviceFilePath, null)
                });
        }

        var parserMode = deviceProfile?.ParserMode;
        if (NidekRtSerialPhoropterParser.IsParserMode(parserMode))
        {
            try
            {
                return new NidekRtSerialPhoropterParser().ParseFile(deviceFilePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                return new DeviceParseResult(
                    Array.Empty<MeasurementValue>(),
                    new[]
                    {
                        new DeviceParseIssue(DeviceParseIssueSeverity.Error, CreateDeviceReadExceptionMessage(ex, deviceFilePath), deviceFilePath, null)
                    });
            }
        }

        if (!string.Equals(parserMode, nameof(DeviceParserMode.Xml), StringComparison.OrdinalIgnoreCase)
            && !string.Equals(Path.GetExtension(deviceFilePath), ".xml", StringComparison.OrdinalIgnoreCase))
        {
            return new DeviceParseResult(
                Array.Empty<MeasurementValue>(),
                new[]
                {
                    new DeviceParseIssue(DeviceParseIssueSeverity.Error, "Dieser Dateityp wird für die Baukasten-Vorschau noch nicht unterstützt.", deviceFilePath, null)
                });
        }

        try
        {
            return _xmlDeviceParser.ParseFile(deviceFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return new DeviceParseResult(
                Array.Empty<MeasurementValue>(),
                new[]
                {
                    new DeviceParseIssue(DeviceParseIssueSeverity.Error, CreateDeviceReadExceptionMessage(ex, deviceFilePath), deviceFilePath, null)
                });
        }
    }

    private AisPatientDataReadForPreviewResult ReadAisPatientData(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile,
        string aisFilePath)
    {
        if (string.IsNullOrWhiteSpace(aisFilePath))
        {
            return AisPatientDataReadForPreviewResult.CreateFailure(
                null,
                Array.Empty<ProcessingIssue>(),
                new[] { "AIS-Dateipfad fehlt." });
        }

        if (!File.Exists(aisFilePath))
        {
            return AisPatientDataReadForPreviewResult.CreateFailure(
                null,
                Array.Empty<ProcessingIssue>(),
                new[] { $"AIS-Datei nicht gefunden: {aisFilePath}" });
        }

        GdtParseResult gdtResult;
        try
        {
            gdtResult = _gdtParser.ParseFile(aisFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return AisPatientDataReadForPreviewResult.CreateFailure(
                null,
                Array.Empty<ProcessingIssue>(),
                new[] { CreateAisReadExceptionMessage(ex, aisFilePath) });
        }

        if (!gdtResult.HasErrors)
        {
            return AisPatientDataReadForPreviewResult.CreateSuccess(
                _patientDataMapper.Map(gdtResult.Records),
                ConvertGdtIssues(gdtResult.Issues, preserveErrorSeverity: true));
        }

        if (InterfaceProfileUiPolicy.IsCv5000(interfaceProfile, deviceProfile)
            || InterfaceProfileUiPolicy.IsNidekRt6100(interfaceProfile, deviceProfile))
        {
            var label = InterfaceProfileUiPolicy.IsNidekRt6100(interfaceProfile, deviceProfile)
                ? "RT-6100"
                : "CV-5000";
            return ReadPhoropterHistoryAisPatientData(aisFilePath, gdtResult, label);
        }

        return AisPatientDataReadForPreviewResult.CreateFailure(
            null,
            ConvertGdtIssues(gdtResult.Issues, preserveErrorSeverity: true),
            CreateGdtFailureMessages(gdtResult.Issues));
    }

    private AisPatientDataReadForPreviewResult ReadPhoropterHistoryAisPatientData(
        string aisFilePath,
        GdtParseResult strictGdtResult,
        string deviceLabel)
    {
        MedistarHistoricalMeasurementParseResult historyResult;
        try
        {
            historyResult = _medistarHistoricalMeasurementParser.ParseFile(aisFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            var messages = CreateGdtFailureMessages(strictGdtResult.Issues).ToList();
            messages.Insert(0, $"{deviceLabel}-Historien-AIS-Datei konnte nicht gelesen werden: {ex.Message}");
            return AisPatientDataReadForPreviewResult.CreateFailure(
                null,
                ConvertGdtIssues(strictGdtResult.Issues, preserveErrorSeverity: true),
                messages);
        }

        var issues = new List<ProcessingIssue>
        {
            new(
                ProcessingIssueSeverity.Warning,
                ProcessingStage.GdtParsing,
                $"{deviceLabel}-Rueckweg: Standard-GDT-Leser meldete MEDISTAR-Historienzeilen; Patientenkontext wurde mit dem Phoropter-Historienparser gelesen.")
        };
        issues.AddRange(ConvertGdtIssues(strictGdtResult.Issues.Take(5), preserveErrorSeverity: false));
        issues.AddRange(historyResult.Warnings.Select(warning => new ProcessingIssue(
            ProcessingIssueSeverity.Warning,
            ProcessingStage.GdtParsing,
            $"{deviceLabel}-Historienparser: {warning}")));

        var missingFields = GetMissingRequiredAisPatientFields(historyResult.Patient);
        if (missingFields.Count > 0)
        {
            return AisPatientDataReadForPreviewResult.CreateFailure(
                historyResult.Patient,
                issues,
                new[]
                {
                    "AIS-Datei enthaelt MEDISTAR-Historienzeilen, aber Pflicht-Patientendaten fehlen: "
                    + string.Join(", ", missingFields)
                });
        }

        return AisPatientDataReadForPreviewResult.CreateSuccess(historyResult.Patient, issues);
    }

    private static void AddFailureMessages(
        List<ProcessingIssue> issues,
        ProcessingStage stage,
        IReadOnlyList<string> failureMessages)
    {
        foreach (var message in failureMessages)
        {
            issues.Add(new ProcessingIssue(ProcessingIssueSeverity.Error, stage, message));
        }
    }

    private static IReadOnlyList<string> GetMissingRequiredAisPatientFields(PatientData patient)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(patient.PatientNumber))
        {
            missing.Add("3000 Patientennummer");
        }

        if (string.IsNullOrWhiteSpace(patient.LastName))
        {
            missing.Add("3101 Nachname");
        }

        if (string.IsNullOrWhiteSpace(patient.FirstName))
        {
            missing.Add("3102 Vorname");
        }

        if (string.IsNullOrWhiteSpace(patient.BirthDate))
        {
            missing.Add("3103 Geburtsdatum");
        }

        return missing;
    }

    private static IReadOnlyList<ProcessingIssue> ConvertGdtIssues(
        IEnumerable<GdtParseIssue> issues,
        bool preserveErrorSeverity)
    {
        return issues
            .Select(issue => new ProcessingIssue(
                preserveErrorSeverity && issue.Severity == GdtParseIssueSeverity.Error
                    ? ProcessingIssueSeverity.Error
                    : ProcessingIssueSeverity.Warning,
                ProcessingStage.GdtParsing,
                FormatGdtIssue(issue)))
            .ToArray();
    }

    private static IReadOnlyList<string> CreateGdtFailureMessages(IEnumerable<GdtParseIssue> issues)
    {
        var details = issues
            .Where(issue => issue.Severity == GdtParseIssueSeverity.Error)
            .Take(5)
            .Select(FormatGdtIssue)
            .ToArray();

        if (details.Length == 0)
        {
            return new[] { "AIS-Datei konnte wegen GDT-/XDT-Parserfehlern nicht gelesen werden." };
        }

        return new[]
        {
            "AIS-Datei konnte wegen GDT-/XDT-Parserfehlern nicht gelesen werden:",
        }
        .Concat(details)
        .ToArray();
    }

    private static string FormatGdtIssue(GdtParseIssue issue)
    {
        var rawLine = TrimForDiagnostic(issue.RawLine);
        return string.IsNullOrWhiteSpace(rawLine)
            ? $"Zeile {issue.LineNumber}: {issue.Message}"
            : $"Zeile {issue.LineNumber}: {issue.Message} Inhalt: {rawLine}";
    }

    private static string TrimForDiagnostic(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim();
        return normalized.Length <= 120 ? normalized : normalized[..117] + "...";
    }

    private static string CreateAisReadExceptionMessage(Exception exception, string aisFilePath)
    {
        return exception switch
        {
            FileNotFoundException => $"AIS-Datei nicht gefunden: {aisFilePath}",
            UnauthorizedAccessException => $"Zugriff auf AIS-Datei verweigert: {exception.Message}",
            IOException => $"AIS-Datei konnte wegen Datei-/IO-Fehler nicht gelesen werden: {exception.Message}",
            ArgumentException or NotSupportedException => $"AIS-Dateipfad ist ungueltig: {exception.Message}",
            _ => $"AIS-Datei konnte nicht gelesen werden: {exception.Message}"
        };
    }

    private static string CreateDeviceReadExceptionMessage(Exception exception, string deviceFilePath)
    {
        return exception switch
        {
            FileNotFoundException => $"Gerätedatei nicht gefunden: {deviceFilePath}",
            UnauthorizedAccessException => $"Zugriff auf Gerätedatei verweigert: {exception.Message}",
            IOException => $"Gerätedatei konnte wegen Datei-/IO-Fehler nicht gelesen werden: {exception.Message}",
            ArgumentException or NotSupportedException => $"Gerätedateipfad ist ungueltig: {exception.Message}",
            _ => $"Gerätedatei konnte nicht gelesen werden: {exception.Message}"
        };
    }

    private sealed record AisPatientDataReadForPreviewResult(
        bool Success,
        PatientData? Patient,
        IReadOnlyList<ProcessingIssue> Issues,
        IReadOnlyList<string> FailureMessages)
    {
        public static AisPatientDataReadForPreviewResult CreateSuccess(
            PatientData patient,
            IReadOnlyList<ProcessingIssue> issues)
        {
            return new AisPatientDataReadForPreviewResult(true, patient, issues, Array.Empty<string>());
        }

        public static AisPatientDataReadForPreviewResult CreateFailure(
            PatientData? patient,
            IReadOnlyList<ProcessingIssue> issues,
            IReadOnlyList<string> failureMessages)
        {
            return new AisPatientDataReadForPreviewResult(false, patient, issues, failureMessages);
        }
    }
}
