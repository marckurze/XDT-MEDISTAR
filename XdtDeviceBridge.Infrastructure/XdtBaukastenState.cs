using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public enum XdtBaukastenResultView
{
    RawXdt,
    AisView,
    DeviceOutput,
    Diagnostics
}

public enum XdtBaukastenRuleDirection
{
    AisExport,
    DeviceOutput
}

public sealed record XdtBaukastenPreviewLine(
    int LineNumber,
    string Text,
    XdtBaukastenResultView ViewKind,
    XdtBaukastenRuleDirection Direction,
    string? RuleId = null,
    int? RuleIndex = null,
    string? RuleName = null,
    string? Target = null,
    bool IsWarning = false,
    bool IsHighlighted = false);

public sealed record XdtBaukastenPreviewDocument(
    XdtBaukastenResultView ViewKind,
    string PlainText,
    IReadOnlyList<XdtBaukastenPreviewLine> Lines);

public sealed record XdtBaukastenLoadedInput(
    string SourcePath,
    string DisplayName,
    string RawText);

public sealed record XdtBaukastenOutputPreview(
    string RawXdt,
    string AisView,
    string DeviceOutput,
    string Diagnostics,
    IReadOnlyList<string> Messages,
    IReadOnlyDictionary<XdtBaukastenResultView, XdtBaukastenPreviewDocument>? Documents = null);

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
    string ExampleValue = "-",
    bool IsPreparedOnly = false);

public sealed record XdtBaukastenStateSnapshot(
    AisProfile? AisProfile,
    DeviceProfileDefinition? DeviceProfile,
    ExportProfileDefinition? SourceExportProfile,
    XdtBaukastenRuleDirection CurrentRuleDirection,
    IReadOnlyList<ExportRuleDefinition> WorkingExportRules,
    IReadOnlyList<ExportRuleDefinition> WorkingDeviceOutputRules,
    XdtBaukastenLoadedInput? AisInput,
    XdtBaukastenLoadedInput? DeviceInput,
    XdtBaukastenLoadedInput? AttachmentInput,
    SerialRawDeviceInput? SerialInput,
    XdtBaukastenPreviewResult? PreviewResult);

public sealed class XdtBaukastenUndoBuffer
{
    private readonly int _capacity;
    private readonly Stack<XdtBaukastenStateSnapshot> _snapshots = new();

    public XdtBaukastenUndoBuffer(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Undo capacity must be greater than zero.");
        }

        _capacity = capacity;
    }

    public bool CanUndo => _snapshots.Count > 0;

    public int Count => _snapshots.Count;

    public void Push(XdtBaukastenStateSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var items = _snapshots.Reverse().Append(snapshot).TakeLast(_capacity).ToArray();
        _snapshots.Clear();
        foreach (var item in items)
        {
            _snapshots.Push(item);
        }
    }

    public bool TryPop(out XdtBaukastenStateSnapshot? snapshot)
    {
        if (_snapshots.Count == 0)
        {
            snapshot = null;
            return false;
        }

        snapshot = _snapshots.Pop();
        return true;
    }
}

public sealed class XdtBaukastenState
{
    private readonly List<ExportRuleDefinition> _workingExportRules = new();
    private readonly List<ExportRuleDefinition> _workingDeviceOutputRules = new();

    public AisProfile? AisProfile { get; private set; }

    public DeviceProfileDefinition? DeviceProfile { get; private set; }

    public ExportProfileDefinition? SourceExportProfile { get; private set; }

    public XdtBaukastenRuleDirection CurrentRuleDirection { get; private set; } = XdtBaukastenRuleDirection.AisExport;

    public IReadOnlyList<ExportRuleDefinition> WorkingExportRules => _workingExportRules;

    public IReadOnlyList<ExportRuleDefinition> WorkingDeviceOutputRules => _workingDeviceOutputRules;

    public IReadOnlyList<ExportRuleDefinition> CurrentWorkingRules =>
        CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
            ? _workingDeviceOutputRules
            : _workingExportRules;

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
        var profileChanged = !string.Equals(DeviceProfile?.Metadata.Id, profile?.Metadata.Id, StringComparison.OrdinalIgnoreCase);
        DeviceProfile = profile;
        SerialInput = null;
        if (profileChanged)
        {
            _workingDeviceOutputRules.Clear();
            _workingDeviceOutputRules.AddRange(XdtBaukastenDeviceOutputRuleService.CreateDefaultRules(profile));
            if (profile?.IsBidirectional != true)
            {
                CurrentRuleDirection = XdtBaukastenRuleDirection.AisExport;
            }
        }
    }

    public void SetRuleDirection(XdtBaukastenRuleDirection direction)
    {
        CurrentRuleDirection = direction == XdtBaukastenRuleDirection.DeviceOutput && !IsBidirectionalDevice
            ? XdtBaukastenRuleDirection.AisExport
            : direction;
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

    public void ClearDeviceInput()
    {
        DeviceInput = null;
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

    public void ClearPreviewResult()
    {
        PreviewResult = null;
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

        var rules = GetCurrentMutableRules();
        var index = rules.FindIndex(existing => string.Equals(existing.Id, rule.Id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return false;
        }

        rules[index] = rule;
        return true;
    }

    public void AddWorkingRule(ExportRuleDefinition rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        GetCurrentMutableRules().Add(rule);
    }

    public bool RemoveWorkingRule(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId))
        {
            return false;
        }

        var rules = GetCurrentMutableRules();
        var index = rules.FindIndex(existing => string.Equals(existing.Id, ruleId, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return false;
        }

        rules.RemoveAt(index);
        return true;
    }

    private List<ExportRuleDefinition> GetCurrentMutableRules()
    {
        return CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
            ? _workingDeviceOutputRules
            : _workingExportRules;
    }

    public XdtBaukastenStateSnapshot CreateSnapshot()
    {
        return new XdtBaukastenStateSnapshot(
            AisProfile,
            DeviceProfile,
            SourceExportProfile,
            CurrentRuleDirection,
            _workingExportRules.ToList(),
            _workingDeviceOutputRules.ToList(),
            AisInput,
            DeviceInput,
            AttachmentInput,
            SerialInput,
            PreviewResult);
    }

    public void RestoreSnapshot(XdtBaukastenStateSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        AisProfile = snapshot.AisProfile;
        DeviceProfile = snapshot.DeviceProfile;
        SourceExportProfile = snapshot.SourceExportProfile;
        CurrentRuleDirection = snapshot.CurrentRuleDirection;
        _workingExportRules.Clear();
        _workingExportRules.AddRange(snapshot.WorkingExportRules);
        _workingDeviceOutputRules.Clear();
        _workingDeviceOutputRules.AddRange(snapshot.WorkingDeviceOutputRules);
        if (!IsBidirectionalDevice && CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput)
        {
            CurrentRuleDirection = XdtBaukastenRuleDirection.AisExport;
        }

        AisInput = snapshot.AisInput;
        DeviceInput = snapshot.DeviceInput;
        AttachmentInput = snapshot.AttachmentInput;
        SerialInput = snapshot.SerialInput;
        PreviewResult = snapshot.PreviewResult;
    }
}
