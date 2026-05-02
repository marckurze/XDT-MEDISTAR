namespace XdtDeviceBridge.Core;

public sealed class ProcessingPipelineService
{
    private readonly GdtParser _gdtParser = new();
    private readonly PatientDataMapper _patientDataMapper = new();
    private readonly XmlDeviceParser _xmlDeviceParser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly XdtExportBuilder _exportBuilder = new();

    public ProcessingPipelineResult ProcessFiles(string gdtFilePath, string deviceFilePath, DeviceProfile profile)
    {
        var issues = new List<ProcessingIssue>();

        var gdtResult = _gdtParser.ParseFile(gdtFilePath);
        issues.AddRange(gdtResult.Issues.Select(i => new ProcessingIssue(
            i.Severity == GdtParseIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.GdtParsing,
            i.Message)));

        if (gdtResult.HasErrors)
        {
            return new ProcessingPipelineResult(null, [], [], string.Empty, issues);
        }

        var patient = _patientDataMapper.Map(gdtResult.Records);

        DeviceParseResult deviceResult;
        if (profile.DeviceParserMode == DeviceParserMode.Xml)
        {
            deviceResult = _xmlDeviceParser.ParseFile(deviceFilePath);
        }
        else
        {
            issues.Add(new ProcessingIssue(ProcessingIssueSeverity.Error, ProcessingStage.DeviceParsing, "Unsupported device parser mode"));
            return new ProcessingPipelineResult(patient, [], [], string.Empty, issues);
        }

        issues.AddRange(deviceResult.Issues.Select(i => new ProcessingIssue(
            i.Severity == DeviceParseIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.DeviceParsing,
            i.Message)));

        if (deviceResult.HasErrors)
        {
            return new ProcessingPipelineResult(patient, deviceResult.Measurements, [], string.Empty, issues);
        }

        var mappingResult = _mappingEngine.Map(patient, deviceResult.Measurements, profile.MappingRules);
        issues.AddRange(mappingResult.Issues.Select(i => new ProcessingIssue(
            i.Severity == MappingIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.Mapping,
            i.Message)));

        if (mappingResult.HasErrors)
        {
            return new ProcessingPipelineResult(patient, deviceResult.Measurements, mappingResult.Records, string.Empty, issues);
        }

        var exportResult = _exportBuilder.Build(mappingResult.Records);
        issues.AddRange(exportResult.Issues.Select(i => new ProcessingIssue(
            i.Severity == XdtExportIssueSeverity.Error ? ProcessingIssueSeverity.Error : ProcessingIssueSeverity.Warning,
            ProcessingStage.Export,
            i.Message)));

        return new ProcessingPipelineResult(patient, deviceResult.Measurements, mappingResult.Records, exportResult.Content, issues);
    }
}
