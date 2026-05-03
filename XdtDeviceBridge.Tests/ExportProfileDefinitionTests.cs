using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class ExportProfileDefinitionTests
{
    [Fact]
    public void CreateMedistarNidekArk1sDefault_ShouldCreateProfile()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();

        Assert.Equal("ais-medistar-default", profile.TargetAisProfileId);
        Assert.Equal("device-nidek-ark1s-default", profile.SourceDeviceProfileId);
        Assert.Equal("Windows-1252", profile.OutputEncoding);
        Assert.Equal(8, profile.Rules.Count);
    }

    [Fact]
    public void Validate_ShouldAcceptDefaultProfile()
    {
        var issues = ExportProfileDefinitionValidator.Validate(DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault());

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ShouldReportWrongProfileKind()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(ProfileKind.DeviceProfile)
        };

        var issues = ExportProfileDefinitionValidator.Validate(profile);

        Assert.Contains("Metadata.ProfileKind must be ExportProfile.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyTargetAisProfileId()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() with { TargetAisProfileId = "" };

        var issues = ExportProfileDefinitionValidator.Validate(profile);

        Assert.Contains("TargetAisProfileId must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptySourceDeviceProfileId()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() with { SourceDeviceProfileId = " " };

        var issues = ExportProfileDefinitionValidator.Validate(profile);

        Assert.Contains("SourceDeviceProfileId must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyOutputEncoding()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() with { OutputEncoding = "" };

        var issues = ExportProfileDefinitionValidator.Validate(profile);

        Assert.Contains("OutputEncoding must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportActiveRuleWithoutTargetFieldCode()
    {
        var profile = WithModifiedRule("1", rule => rule with { TargetFieldCode = "" });

        var issues = ExportProfileDefinitionValidator.Validate(profile);

        Assert.Contains("Rule 1: TargetFieldCode must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportActiveRuleWithoutOutputTemplate()
    {
        var profile = WithModifiedRule("1", rule => rule with { OutputTemplate = "" });

        var issues = ExportProfileDefinitionValidator.Validate(profile);

        Assert.Contains("Rule 1: OutputTemplate must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportDuplicateSortOrderForActiveRules()
    {
        var profile = WithModifiedRule("2", rule => rule with { SortOrder = 1 });

        var issues = ExportProfileDefinitionValidator.Validate(profile);

        Assert.Contains("Active rules contain duplicate SortOrder: 1", issues);
    }

    [Fact]
    public void CreateMedistarNidekArk1sDefault_ShouldContainStaticValueRuleFor8000()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();

        Assert.Contains(profile.Rules, rule =>
            rule.TargetFieldCode == "8000"
            && rule.RuleType == ExportRuleType.StaticValue
            && rule.SourcePath is null
            && rule.OutputTemplate == "6310");
    }

    [Fact]
    public void CreateMedistarNidekArk1sDefault_ShouldContainAisFieldRuleFor8402()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();

        Assert.Contains(profile.Rules, rule =>
            rule.TargetFieldCode == "8402"
            && rule.RuleType == ExportRuleType.AisField
            && rule.SourcePath == "AIS.ExaminationType");
    }

    [Fact]
    public void CreateMedistarNidekArk1sDefault_ShouldContainTwoTemplateRulesFor6228()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var resultRules = profile.Rules
            .Where(rule => rule.TargetFieldCode == "6228" && rule.RuleType == ExportRuleType.Template)
            .ToList();

        Assert.Equal(2, resultRules.Count);
        Assert.Contains(resultRules, rule => rule.OutputTemplate.Contains("R.:S="));
        Assert.Contains(resultRules, rule => rule.OutputTemplate.Contains("L.:S="));
        Assert.All(resultRules, rule => Assert.Contains("PD=", rule.OutputTemplate));
    }

    private static ExportProfileDefinition WithModifiedRule(
        string id,
        Func<ExportRuleDefinition, ExportRuleDefinition> modify)
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();

        var rules = profile.Rules
            .Select(rule => rule.Id == id ? modify(rule) : rule)
            .ToList();

        return profile with { Rules = rules };
    }

    private static ProfileMetadata CreateMetadata(ProfileKind profileKind)
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new ProfileMetadata(
            Id: "metadata-test",
            Name: "Metadata Test",
            ProfileKind: profileKind,
            Description: null,
            Vendor: null,
            Product: null,
            Version: "1.0.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "XdtDeviceBridge",
            IsBuiltIn: false,
            IsUserDefined: true);
    }
}
