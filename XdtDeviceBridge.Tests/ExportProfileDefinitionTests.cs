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

    [Fact]
    public void CreateMedistarNidekLm7Default_ShouldCreateProfile()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default();

        Assert.Equal("ais-medistar-default", profile.TargetAisProfileId);
        Assert.Equal("device-nidek-lm7-default", profile.SourceDeviceProfileId);
        Assert.Equal("Windows-1252", profile.OutputEncoding);
        Assert.Equal(8, profile.Rules.Count);
    }

    [Fact]
    public void Validate_ShouldAcceptMedistarNidekLm7DefaultProfile()
    {
        var issues = ExportProfileDefinitionValidator.Validate(DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default());

        Assert.Empty(issues);
    }

    [Fact]
    public void CreateMedistarNidekLm7Default_ShouldContainTwoTemplateRulesFor6228()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default();
        var resultRules = profile.Rules
            .Where(rule => rule.TargetFieldCode == "6228" && rule.RuleType == ExportRuleType.Template)
            .ToList();

        Assert.Equal(2, resultRules.Count);
    }

    [Fact]
    public void CreateMedistarNidekLm7Default_ShouldContainLensmeterTemplateParts()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default();
        var rightTemplate = profile.Rules.Single(rule => rule.TargetName == "LensmeterResultRight").OutputTemplate;
        var leftTemplate = profile.Rules.Single(rule => rule.TargetName == "LensmeterResultLeft").OutputTemplate;

        Assert.Contains("P=", rightTemplate);
        Assert.Contains("PD=", rightTemplate);
        Assert.Contains("P=", leftTemplate);
    }

    [Fact]
    public void CreateMedistarNidekLm7Default_ShouldContainStaticValueRuleFor8000()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default();

        Assert.Contains(profile.Rules, rule =>
            rule.TargetFieldCode == "8000"
            && rule.RuleType == ExportRuleType.StaticValue
            && rule.SourcePath is null
            && rule.OutputTemplate == "6310");
    }

    [Fact]
    public void CreateMedistarNidekLm7Default_ShouldContainAisFieldRuleFor8402()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default();

        Assert.Contains(profile.Rules, rule =>
            rule.TargetFieldCode == "8402"
            && rule.RuleType == ExportRuleType.AisField
            && rule.SourcePath == "AIS.ExaminationType");
    }

    [Fact]
    public void CreateMedistarNidekNt530PDefault_ShouldCreateProfile()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault();

        Assert.Equal("ais-medistar-default", profile.TargetAisProfileId);
        Assert.Equal("device-nidek-nt530p-default", profile.SourceDeviceProfileId);
        Assert.Equal("Windows-1252", profile.OutputEncoding);
        Assert.Equal(9, profile.Rules.Count);
    }

    [Fact]
    public void Validate_ShouldAcceptMedistarNidekNt530PDefaultProfile()
    {
        var issues = ExportProfileDefinitionValidator.Validate(DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault());

        Assert.Empty(issues);
    }

    [Fact]
    public void CreateMedistarNidekNt530PDefault_ShouldContainStaticValueRuleFor8000()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault();

        Assert.Contains(profile.Rules, rule =>
            rule.TargetFieldCode == "8000"
            && rule.RuleType == ExportRuleType.StaticValue
            && rule.SourcePath is null
            && rule.OutputTemplate == "6310");
    }

    [Fact]
    public void CreateMedistarNidekNt530PDefault_ShouldContainAisFieldRuleFor8402()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault();

        Assert.Contains(profile.Rules, rule =>
            rule.TargetFieldCode == "8402"
            && rule.RuleType == ExportRuleType.AisField
            && rule.SourcePath == "AIS.ExaminationType");
    }

    [Fact]
    public void CreateMedistarNidekNt530PDefault_ShouldContainMultipleResultRulesFor6228()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault();
        var resultRules = profile.Rules
            .Where(rule => rule.TargetFieldCode == "6228" && rule.RuleType == ExportRuleType.Template)
            .ToList();

        Assert.Equal(3, resultRules.Count);
        Assert.Contains(resultRules, rule => rule.OutputTemplate.Contains("mmHg"));
        Assert.Contains(resultRules, rule => rule.OutputTemplate.Contains("µm"));
    }

    [Fact]
    public void CreateMedistarNidekNt530PDefault_ShouldDescribeFutureEvAttachmentExtension()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault();

        Assert.Contains("EV", profile.Metadata.Description);
        Assert.Contains("Attachment", profile.Metadata.Description);
    }

    [Fact]
    public void CreateMedistarTopconCl300Default_ShouldCreateProfile()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconCl300Default();

        Assert.Equal("ais-medistar-default", profile.TargetAisProfileId);
        Assert.Equal("device-topcon-cl300-default", profile.SourceDeviceProfileId);
        Assert.Equal("Windows-1252", profile.OutputEncoding);
        Assert.Equal(8, profile.Rules.Count);
    }

    [Fact]
    public void Validate_ShouldAcceptMedistarTopconCl300DefaultProfile()
    {
        var issues = ExportProfileDefinitionValidator.Validate(DefaultExportProfileDefinitions.CreateMedistarTopconCl300Default());

        Assert.Empty(issues);
    }

    [Fact]
    public void CreateMedistarTopconCl300Default_ShouldContainStaticValueRuleFor8000()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconCl300Default();

        Assert.Contains(profile.Rules, rule =>
            rule.TargetFieldCode == "8000"
            && rule.RuleType == ExportRuleType.StaticValue
            && rule.SourcePath is null
            && rule.OutputTemplate == "6310");
    }

    [Fact]
    public void CreateMedistarTopconCl300Default_ShouldContainAisFieldRuleFor8402()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconCl300Default();

        Assert.Contains(profile.Rules, rule =>
            rule.TargetFieldCode == "8402"
            && rule.RuleType == ExportRuleType.AisField
            && rule.SourcePath == "AIS.ExaminationType");
    }

    [Fact]
    public void CreateMedistarTopconCl300Default_ShouldContainTwoTemplateRulesFor6228()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconCl300Default();
        var resultRules = profile.Rules
            .Where(rule => rule.TargetFieldCode == "6228" && rule.RuleType == ExportRuleType.Template)
            .ToList();

        Assert.Equal(2, resultRules.Count);
        Assert.Contains(resultRules, rule => rule.OutputTemplate.Contains("R.:S="));
        Assert.Contains(resultRules, rule => rule.OutputTemplate.Contains("L.:S="));
    }

    [Fact]
    public void CreateMedistarTopconKr800Default_ShouldCreateProfile()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default();

        Assert.Equal("ais-medistar-default", profile.TargetAisProfileId);
        Assert.Equal("device-topcon-kr800-default", profile.SourceDeviceProfileId);
        Assert.Equal("Windows-1252", profile.OutputEncoding);
        Assert.Equal(10, profile.Rules.Count);
    }

    [Fact]
    public void Validate_ShouldAcceptMedistarTopconKr800DefaultProfile()
    {
        var issues = ExportProfileDefinitionValidator.Validate(DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default());

        Assert.Empty(issues);
    }

    [Fact]
    public void CreateMedistarTopconKr800Default_ShouldContainStaticValueRuleFor8000()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default();

        Assert.Contains(profile.Rules, rule =>
            rule.TargetFieldCode == "8000"
            && rule.RuleType == ExportRuleType.StaticValue
            && rule.SourcePath is null
            && rule.OutputTemplate == "6310");
    }

    [Fact]
    public void CreateMedistarTopconKr800Default_ShouldContainAisFieldRuleFor8402()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default();

        Assert.Contains(profile.Rules, rule =>
            rule.TargetFieldCode == "8402"
            && rule.RuleType == ExportRuleType.AisField
            && rule.SourcePath == "AIS.ExaminationType");
    }

    [Fact]
    public void CreateMedistarTopconKr800Default_ShouldContainRefTemplateRulesFor6228()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default();
        var refRules = profile.Rules
            .Where(rule =>
                rule.TargetFieldCode == "6228"
                && rule.RuleType == ExportRuleType.Template
                && rule.TargetName.StartsWith("Ref", StringComparison.Ordinal))
            .ToList();

        Assert.Equal(2, refRules.Count);
        Assert.Contains(refRules, rule => rule.OutputTemplate.Contains("R.:S="));
        Assert.Contains(refRules, rule => rule.OutputTemplate.Contains("L.:S="));
        Assert.All(refRules, rule => Assert.Contains(":Diopter", rule.OutputTemplate));
        Assert.All(refRules, rule => Assert.Contains(":Axis", rule.OutputTemplate));
        Assert.All(refRules, rule => Assert.Contains(":Pd", rule.OutputTemplate));
    }

    [Fact]
    public void CreateMedistarTopconKr800Default_ShouldPrepareProvisionalKmRules()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default();
        var kmRules = profile.Rules
            .Where(rule =>
                rule.TargetFieldCode == "6228"
                && rule.RuleType == ExportRuleType.Template
                && rule.TargetName.Contains("Keratometry", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Equal(2, kmRules.Count);
        Assert.All(kmRules, rule => Assert.Contains(":Keratometry", rule.OutputTemplate));
        Assert.All(kmRules, rule => Assert.Contains("KM-Ausgabe noch zu validieren", rule.Description ?? string.Empty));
    }

    [Fact]
    public void CreateMedistarTopconTrk2PDefault_ShouldCreateProfile()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault();

        Assert.Equal("ais-medistar-default", profile.TargetAisProfileId);
        Assert.Equal("device-topcon-trk2p-default", profile.SourceDeviceProfileId);
        Assert.Equal("Windows-1252", profile.OutputEncoding);
        Assert.Equal(9, profile.Rules.Count);
    }

    [Fact]
    public void Validate_ShouldAcceptMedistarTopconTrk2PDefaultProfile()
    {
        var issues = ExportProfileDefinitionValidator.Validate(DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault());

        Assert.Empty(issues);
    }

    [Fact]
    public void CreateMedistarTopconTrk2PDefault_ShouldContainStaticValueRuleFor8000()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault();

        Assert.Contains(profile.Rules, rule =>
            rule.TargetFieldCode == "8000"
            && rule.RuleType == ExportRuleType.StaticValue
            && rule.SourcePath is null
            && rule.OutputTemplate == "6310");
    }

    [Fact]
    public void CreateMedistarTopconTrk2PDefault_ShouldContainAisFieldRuleFor8402()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault();

        Assert.Contains(profile.Rules, rule =>
            rule.TargetFieldCode == "8402"
            && rule.RuleType == ExportRuleType.AisField
            && rule.SourcePath == "AIS.ExaminationType");
    }

    [Fact]
    public void CreateMedistarTopconTrk2PDefault_ShouldContainMultipleResultRulesFor6228()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault();
        var resultRules = profile.Rules
            .Where(rule => rule.TargetFieldCode == "6228" && rule.RuleType == ExportRuleType.Template)
            .ToList();

        Assert.Equal(3, resultRules.Count);
        Assert.Contains(resultRules, rule => rule.OutputTemplate.Contains("mmHg"));
        Assert.Contains(resultRules, rule => rule.OutputTemplate.Contains("µm"));
        Assert.Contains(resultRules, rule => rule.OutputTemplate.Contains(":Iop"));
        Assert.Contains(resultRules, rule => rule.OutputTemplate.Contains(":Pachy"));
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
