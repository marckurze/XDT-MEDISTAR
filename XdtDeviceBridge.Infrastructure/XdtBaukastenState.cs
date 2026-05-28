using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public enum XdtBaukastenResultView
{
    RawXdt,
    Readable,
    DeviceOutput
}

public sealed record XdtBaukastenLoadedInput(
    string SourcePath,
    string DisplayName,
    string RawText);

public sealed record XdtBaukastenOutputPreview(
    string RawXdt,
    string Readable,
    string DeviceOutput,
    IReadOnlyList<string> Messages);

public sealed record XdtBaukastenPreviewResult(
    bool Success,
    ProcessingPipelineResult? PipelineResult,
    XdtBaukastenOutputPreview Output,
    IReadOnlyList<string> Messages);

public sealed record XdtBaukastenPlaceholder(
    string Category,
    string DisplayName,
    string Token,
    string Description,
    bool IsPreparedOnly = false);

public sealed class XdtBaukastenState
{
    private readonly List<ExportRuleDefinition> _workingExportRules = new();

    public AisProfile? AisProfile { get; private set; }

    public DeviceProfileDefinition? DeviceProfile { get; private set; }

    public ExportProfileDefinition? SourceExportProfile { get; private set; }

    public IReadOnlyList<ExportRuleDefinition> WorkingExportRules => _workingExportRules;

    public XdtBaukastenLoadedInput? AisInput { get; private set; }

    public XdtBaukastenLoadedInput? DeviceInput { get; private set; }

    public XdtBaukastenLoadedInput? AttachmentInput { get; private set; }

    public SerialRawDeviceInput? SerialInput { get; private set; }

    public XdtBaukastenPreviewResult? PreviewResult { get; private set; }

    public bool IsSerialDevice => DeviceProfile?.ConnectionKind == DeviceConnectionKind.SerialRs232;

    public bool IsBidirectionalDevice => DeviceProfile?.IsBidirectional == true;

    public string PrimaryInputButtonText => IsSerialDevice ? "COM Port abhören" : "AIS Datei laden";

    public string PrimaryRawInputTitle => IsSerialDevice
        ? "Anzeige empfangene RS232-Rohdaten"
        : "Anzeige Rohdaten von AIS Datei";

    public void SetAisProfile(AisProfile? profile)
    {
        AisProfile = profile;
    }

    public void SetDeviceProfile(DeviceProfileDefinition? profile)
    {
        DeviceProfile = profile;
        SerialInput = null;
    }

    public void SetExportProfile(ExportProfileDefinition? profile)
    {
        SourceExportProfile = profile;
        _workingExportRules.Clear();

        if (profile is null)
        {
            return;
        }

        foreach (var rule in profile.Rules.OrderBy(rule => rule.SortOrder).ThenBy(rule => rule.Id, StringComparer.OrdinalIgnoreCase))
        {
            _workingExportRules.Add(rule);
        }
    }

    public void SetAisInput(XdtBaukastenLoadedInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        AisInput = input;
        SerialInput = null;
    }

    public void SetSerialInput(SerialRawDeviceInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        SerialInput = input;
        AisInput = new XdtBaukastenLoadedInput(
            SourcePath: input.PortName,
            DisplayName: $"{input.PortName} ({input.ReceivedAt:dd.MM.yyyy HH:mm:ss})",
            RawText: string.IsNullOrWhiteSpace(input.HexDump)
                ? input.RawText
                : input.RawText + Environment.NewLine + Environment.NewLine + "Hexdump:" + Environment.NewLine + input.HexDump);
    }

    public void SetDeviceInput(XdtBaukastenLoadedInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        DeviceInput = input;
    }

    public void SetAttachmentInput(XdtBaukastenLoadedInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        AttachmentInput = input;
    }

    public void SetPreviewResult(XdtBaukastenPreviewResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        PreviewResult = result;
    }

    public ExportProfileDefinition? CreateWorkingExportProfile()
    {
        return SourceExportProfile is null
            ? null
            : SourceExportProfile with { Rules = _workingExportRules.ToList() };
    }

    public bool UpdateWorkingRule(ExportRuleDefinition rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        var index = _workingExportRules.FindIndex(existing => string.Equals(existing.Id, rule.Id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return false;
        }

        _workingExportRules[index] = rule;
        return true;
    }
}
