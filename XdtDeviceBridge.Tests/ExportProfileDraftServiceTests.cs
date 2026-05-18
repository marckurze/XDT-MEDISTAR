using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class ExportProfileDraftServiceTests
{
    private readonly ExportProfileDraftService _service = new();
    private readonly DateTimeOffset _timestamp = new(2026, 5, 5, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateUserDefinedCopy_ShouldKeepOriginalProfileUnchanged()
    {
        var original = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var originalRule = original.Rules.Single(rule => rule.Id == "7");
        var draftRule = originalRule with { OutputTemplate = "R.: geändert" };

        var result = CreateCopy(original, draftRule, replaceRuleId: "7");

        Assert.True(result.Success);
        Assert.Equal(originalRule.OutputTemplate, original.Rules.Single(rule => rule.Id == "7").OutputTemplate);
        Assert.NotEqual(original.Rules.Single(rule => rule.Id == "7").OutputTemplate, result.Profile!.Rules.Single(rule => rule.Id == "7").OutputTemplate);
    }

    [Fact]
    public void CreateUserDefinedCopy_ShouldCreateNewUserDefinedMetadata()
    {
        var original = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();

        var result = CreateCopy(original);

        Assert.True(result.Success);
        var metadata = result.Profile!.Metadata;
        Assert.Equal("export-test-copy", metadata.Id);
        Assert.NotEqual(original.Metadata.Id, metadata.Id);
        Assert.Equal("MEDISTAR + NIDEK ARK1S Export - Kopie", metadata.Name);
        Assert.Equal(ProfileKind.ExportProfile, metadata.ProfileKind);
        Assert.Equal("Benutzerdefinierte Kopie von MEDISTAR + NIDEK ARK1S Export", metadata.Description);
        Assert.Equal(original.Metadata.Vendor, metadata.Vendor);
        Assert.Equal(original.Metadata.Product, metadata.Product);
        Assert.Equal("1.0", metadata.Version);
        Assert.Equal(_timestamp, metadata.CreatedAt);
        Assert.Equal(_timestamp, metadata.UpdatedAt);
        Assert.Equal("TestUser", metadata.CreatedBy);
        Assert.False(metadata.IsBuiltIn);
        Assert.True(metadata.IsUserDefined);
    }

    [Fact]
    public void CreateUserDefinedCopy_ShouldApplyChangedDraftRule()
    {
        var original = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var draftRule = original.Rules.Single(rule => rule.Id == "7") with
        {
            OutputTemplate = "R.:S={Device.R/AR/ARMedian/Sphere:Diopter}"
        };

        var result = CreateCopy(original, draftRule, replaceRuleId: "7");

        Assert.True(result.Success);
        Assert.Equal("R.:S={Device.R/AR/ARMedian/Sphere:Diopter}", result.Profile!.Rules.Single(rule => rule.Id == "7").OutputTemplate);
    }

    [Fact]
    public void CreateUserDefinedCopy_ShouldAddTemporaryRules()
    {
        var original = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var temporaryRule = new ExportRuleDefinition(
            Id: "draft-rule-1",
            TargetFieldCode: "6228",
            TargetName: "Zusatz",
            RuleType: ExportRuleType.Template,
            SourcePath: null,
            OutputTemplate: "Zusatz={AIS.LastName}",
            SortOrder: 99,
            IsEnabled: true,
            Description: "temporär");

        var result = CreateCopy(original, temporaryRules: new[] { temporaryRule });

        Assert.True(result.Success);
        Assert.Contains(result.Profile!.Rules, rule => rule.Id == "draft-rule-1" && rule.OutputTemplate == "Zusatz={AIS.LastName}");
    }

    [Fact]
    public void CreateUserDefinedCopy_ShouldCreateEmptyDraftWhenOriginalRulesAreExcluded()
    {
        var original = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();

        var result = _service.CreateUserDefinedCopy(
            original,
            "Leerer Exportprofil-Entwurf",
            draftRule: null,
            replaceRuleId: null,
            temporaryRules: Array.Empty<ExportRuleDefinition>(),
            timestamp: _timestamp,
            createdBy: "TestUser",
            idFactory: () => "export-empty-draft",
            includeOriginalRules: false);

        Assert.True(result.Success);
        Assert.Empty(result.Profile!.Rules);
        Assert.Equal(original.TargetAisProfileId, result.Profile.TargetAisProfileId);
        Assert.Equal(original.SourceDeviceProfileId, result.Profile.SourceDeviceProfileId);
        Assert.Equal(original.OutputEncoding, result.Profile.OutputEncoding);
        Assert.True(original.Rules.Count > 0);
    }

    [Fact]
    public void CreateUserDefinedCopy_ShouldAddTemporaryRulesToEmptyDraft()
    {
        var original = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var temporaryRule = new ExportRuleDefinition(
            Id: "draft-rule-1",
            TargetFieldCode: "6228",
            TargetName: "Zusatz",
            RuleType: ExportRuleType.Template,
            SourcePath: null,
            OutputTemplate: "Zusatz={AIS.LastName}",
            SortOrder: 1,
            IsEnabled: true,
            Description: "temporär");

        var result = _service.CreateUserDefinedCopy(
            original,
            "Exportprofil mit neuer Regel",
            draftRule: null,
            replaceRuleId: null,
            temporaryRules: new[] { temporaryRule },
            timestamp: _timestamp,
            createdBy: "TestUser",
            idFactory: () => "export-new-rule",
            includeOriginalRules: false);

        Assert.True(result.Success);
        Assert.Single(result.Profile!.Rules);
        Assert.Equal("draft-rule-1", result.Profile.Rules[0].Id);
    }

    [Fact]
    public void CreateUserDefinedCopy_ShouldRejectEmptyTargetFieldCode()
    {
        var original = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var draftRule = original.Rules.Single(rule => rule.Id == "7") with { TargetFieldCode = "" };

        var result = CreateCopy(original, draftRule, replaceRuleId: "7");

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue => issue.Contains("TargetFieldCode darf nicht leer sein.", StringComparison.Ordinal));
    }

    [Fact]
    public void CreateUserDefinedCopy_ShouldRejectEmptyOutputTemplate()
    {
        var original = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var draftRule = original.Rules.Single(rule => rule.Id == "7") with { OutputTemplate = "" };

        var result = CreateCopy(original, draftRule, replaceRuleId: "7");

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue => issue.Contains("OutputTemplate darf nicht leer sein.", StringComparison.Ordinal));
    }

    [Fact]
    public void CreateUserDefinedCopy_ShouldRejectDuplicateSortOrder()
    {
        var original = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var draftRule = original.Rules.Single(rule => rule.Id == "7") with { SortOrder = 8 };

        var result = CreateCopy(original, draftRule, replaceRuleId: "7");

        Assert.False(result.Success);
        Assert.Contains("Mehrere Regeln verwenden dieselbe SortOrder: 8", result.Issues);
    }

    [Fact]
    public void CreateUserDefinedCopy_ShouldRejectDeviceFieldWithoutSourcePath()
    {
        var original = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var draftRule = original.Rules.Single(rule => rule.Id == "7") with
        {
            RuleType = ExportRuleType.DeviceField,
            SourcePath = null
        };

        var result = CreateCopy(original, draftRule, replaceRuleId: "7");

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue => issue.Contains("SourcePath darf bei DeviceField nicht leer sein.", StringComparison.Ordinal));
    }

    [Fact]
    public void CreateUserDefinedCopy_ShouldRejectInvalidRuleType()
    {
        var original = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var draftRule = original.Rules.Single(rule => rule.Id == "7") with
        {
            RuleType = (ExportRuleType)999
        };

        var result = CreateCopy(original, draftRule, replaceRuleId: "7");

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue => issue.Contains("RuleType ist ungültig.", StringComparison.Ordinal));
    }

    private ExportProfileDraftResult CreateCopy(
        ExportProfileDefinition original,
        ExportRuleDefinition? draftRule = null,
        string? replaceRuleId = null,
        IEnumerable<ExportRuleDefinition>? temporaryRules = null)
    {
        return _service.CreateUserDefinedCopy(
            original,
            "MEDISTAR + NIDEK ARK1S Export - Kopie",
            draftRule,
            replaceRuleId,
            temporaryRules ?? Array.Empty<ExportRuleDefinition>(),
            _timestamp,
            "TestUser",
            idFactory: () => "export-test-copy");
    }
}
