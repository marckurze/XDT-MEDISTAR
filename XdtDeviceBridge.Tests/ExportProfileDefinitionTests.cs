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

        var rightRule = profile.Rules.Single(rule => rule.TargetName == "LensmeterResultRight");
        var leftRule = profile.Rules.Single(rule => rule.TargetName == "LensmeterResultLeft");

        Assert.Equal("{value}", rightTemplate);
        Assert.Equal("{value}", leftTemplate);
        Assert.Equal("Device.Measure[@Type='LM']/LM/R/MedistarLine", rightRule.SourcePath);
        Assert.Equal("Device.Measure[@Type='LM']/LM/L/MedistarLine", leftRule.SourcePath);
        Assert.DoesNotContain("Device.R/LM/Median", rightTemplate);
        Assert.DoesNotContain("Device.L/LM/Median", leftTemplate);
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
        Assert.Equal(16, profile.Rules.Count);
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
    public void CreateMedistarNidekNt530PDefault_ShouldContainPachyAndTonoResultRulesWithout6228()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault();
        var pachyRules = profile.Rules.Where(rule => rule.TargetFieldCode == "6220").ToList();
        var tonoRules = profile.Rules.Where(rule => rule.TargetFieldCode == "6205").ToList();

        Assert.Equal(2, pachyRules.Count);
        Assert.Equal(8, tonoRules.Count);
        Assert.All(pachyRules, rule => Assert.Equal(ExportRuleType.Template, rule.RuleType));
        Assert.All(tonoRules, rule => Assert.Equal(ExportRuleType.Template, rule.RuleType));
        Assert.Contains(pachyRules, rule => rule.SourcePath == "Device.Measure[@Type='NT530P']/Pachy/HeaderLine");
        Assert.Contains(pachyRules, rule => rule.SourcePath == "Device.Measure[@Type='NT530P']/Pachy/MedistarLine");
        Assert.Contains(tonoRules, rule => rule.SourcePath == "Device.Measure[@Type='NT530P']/Tono/HeaderLine");
        Assert.Contains(tonoRules, rule => rule.SourcePath == "Device.Measure[@Type='NT530P']/Tono/PachyRightLine");
        Assert.Contains(tonoRules, rule => rule.SourcePath == "Device.Measure[@Type='NT530P']/Tono/PachyLeftLine");
        Assert.Contains(tonoRules, rule => rule.SourcePath == "Device.Measure[@Type='NT530P']/Tono/TonoListLine");
        Assert.All(pachyRules.Concat(tonoRules), rule => Assert.Equal("{value}", rule.OutputTemplate));
        Assert.DoesNotContain(profile.Rules, rule => rule.TargetFieldCode == "6228");
    }

    [Fact]
    public void CreateMedistarNidekNt530PDefault_ShouldDocumentPachyAndTonoFieldCodes()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault();

        Assert.Contains("tonometry", profile.Metadata.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("pachymetry", profile.Metadata.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateMedistarDocumentAttachmentDefault_ShouldContainOnlyBaseAndOptionalDocumentationRules()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarDocumentAttachmentDefault();

        Assert.Equal("export-medistar-document-attachment-default", profile.Metadata.Id);
        Assert.Equal("device-document-attachment-default", profile.SourceDeviceProfileId);
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6227" && rule.SourcePath == "Device.AttachmentOnly/DocumentationText");
        Assert.DoesNotContain(profile.Rules, rule => rule.TargetFieldCode is "6228" or "6205" or "6220");
        Assert.Empty(ExportProfileDefinitionValidator.Validate(profile));
    }

    [Fact]
    public void CreateMedistarManualDocumentTransferDefault_ShouldContainOnlyBaseRules()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarManualDocumentTransferDefault();

        Assert.Equal("export-medistar-manual-document-transfer-default", profile.Metadata.Id);
        Assert.Equal("device-manual-document-selection-default", profile.SourceDeviceProfileId);
        Assert.Equal(6, profile.Rules.Count);
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "8402" && rule.SourcePath == "AIS.ExaminationType");
        Assert.DoesNotContain(profile.Rules, rule => rule.TargetFieldCode is "6227" or "6228" or "6205" or "6220");
        Assert.Empty(ExportProfileDefinitionValidator.Validate(profile));
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
        Assert.Contains(resultRules, rule =>
            rule.SourcePath == "Device.Measure[@Type='LM']/LM/R/MedistarLine"
            && rule.OutputTemplate == "{value}");
        Assert.Contains(resultRules, rule =>
            rule.SourcePath == "Device.Measure[@Type='LM']/LM/L/MedistarLine"
            && rule.OutputTemplate == "{value}");
        Assert.DoesNotContain(profile.Rules, rule => rule.TargetFieldCode is "6205" or "6220");
    }

    [Fact]
    public void CreateMedistarTopconKr800Default_ShouldCreateProfile()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default();

        Assert.Equal("ais-medistar-default", profile.TargetAisProfileId);
        Assert.Equal("device-topcon-kr800-default", profile.SourceDeviceProfileId);
        Assert.Equal("Windows-1252", profile.OutputEncoding);
        Assert.Equal(14, profile.Rules.Count);
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
        Assert.Contains(refRules, rule => rule.SourcePath == "Device.Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains(refRules, rule => rule.SourcePath == "Device.Measure[@Type='REF']/REF/L/MedistarLine");
        Assert.All(refRules, rule => Assert.Equal("{value}", rule.OutputTemplate));
    }

    [Fact]
    public void CreateMedistarTopconKr800Default_ShouldContainKmRulesFor6221AndSbjRulesFor6227()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default();
        var kmRules = profile.Rules
            .Where(rule =>
                rule.TargetFieldCode == "6221"
                && rule.RuleType == ExportRuleType.Template
                && rule.TargetName.Contains("Keratometry", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Equal(2, kmRules.Count);
        Assert.Contains(kmRules, rule => rule.SourcePath == "Device.Measure[@Type='KM']/KM/MedistarLine1");
        Assert.Contains(kmRules, rule => rule.SourcePath == "Device.Measure[@Type='KM']/KM/MedistarLine2");

        var sbjRules = profile.Rules
            .Where(rule => rule.TargetFieldCode == "6227" && rule.RuleType == ExportRuleType.Template)
            .ToList();
        Assert.Equal(4, sbjRules.Count);
        Assert.Contains(sbjRules, rule => rule.SourcePath == "Device.Measure[@Type='SBJ']/MedistarLine1");
        Assert.Contains(sbjRules, rule => rule.SourcePath == "Device.Measure[@Type='SBJ']/MedistarLine2");
        Assert.DoesNotContain(profile.Rules, rule => rule.TargetFieldCode is "6205" or "6220" or "6302" or "6303" or "6304" or "6305");
    }

    [Fact]
    public void CreateMedistarTopconTrk2PDefault_ShouldCreateProfile()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault();

        Assert.Equal("ais-medistar-default", profile.TargetAisProfileId);
        Assert.Equal("device-topcon-trk2p-default", profile.SourceDeviceProfileId);
        Assert.Equal("Windows-1252", profile.OutputEncoding);
        Assert.Equal(19, profile.Rules.Count);
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
    public void CreateMedistarTopconTrk2PDefault_ShouldContainPreparedResultRulesForRefKmPachyTonoAndSbj()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault();

        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='REF']/REF/L/MedistarLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6221" && rule.SourcePath == "Device.Measure[@Type='KM']/KM/MedistarLine1");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6221" && rule.SourcePath == "Device.Measure[@Type='KM']/KM/MedistarLine2");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6220" && rule.SourcePath == "Device.Measure[@Type='CCT']/Pachy/MedistarLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='TM']/Tono/TonoListLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6227" && rule.SourcePath == "Device.Measure[@Type='SBJ']/MedistarLine1");
        Assert.DoesNotContain(profile.Rules, rule => rule.TargetFieldCode is "6302" or "6303" or "6304" or "6305");
        Assert.DoesNotContain(profile.Rules, rule => (rule.OutputTemplate ?? string.Empty).Contains(":Iop", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(profile.Rules, rule => (rule.OutputTemplate ?? string.Empty).Contains(":Pachy", StringComparison.OrdinalIgnoreCase));
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
