using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class ExportProfileMappingAdapterTests
{
    private readonly ExportProfileMappingAdapter _adapter = new();

    [Fact]
    public void Adapt_ShouldMapStaticValueRule()
    {
        var profile = CreateExportProfile(new ExportRuleDefinition(
            Id: "static-1",
            TargetFieldCode: "8000",
            TargetName: "MessageType",
            RuleType: ExportRuleType.StaticValue,
            SourcePath: null,
            OutputTemplate: "6310",
            SortOrder: 1,
            IsEnabled: true,
            Description: null));

        var rule = Assert.Single(_adapter.Adapt(profile));

        Assert.Equal("static-1", rule.Id);
        Assert.Equal("8000", rule.TargetFieldCode);
        Assert.Equal("MessageType", rule.TargetName);
        Assert.Equal("AIS.PatientNumber", rule.SourcePath);
        Assert.Equal("6310", rule.OutputTemplate);
    }

    [Fact]
    public void Adapt_ShouldMapAisFieldRule()
    {
        var profile = CreateExportProfile(new ExportRuleDefinition(
            Id: "ais-1",
            TargetFieldCode: "8402",
            TargetName: "ExaminationType",
            RuleType: ExportRuleType.AisField,
            SourcePath: "AIS.ExaminationType",
            OutputTemplate: "{value}",
            SortOrder: 6,
            IsEnabled: true,
            Description: null));

        var rule = Assert.Single(_adapter.Adapt(profile));

        Assert.Equal("8402", rule.TargetFieldCode);
        Assert.Equal("AIS.ExaminationType", rule.SourcePath);
        Assert.Equal("{value}", rule.OutputTemplate);
        Assert.Equal(6, rule.SortOrder);
    }

    [Fact]
    public void Adapt_ShouldKeepEmptySourceForLiteralTemplateRule()
    {
        var profile = CreateExportProfile(new ExportRuleDefinition(
            Id: "template-1",
            TargetFieldCode: "6228",
            TargetName: "ResultRight",
            RuleType: ExportRuleType.Template,
            SourcePath: null,
            OutputTemplate: "R.:S={Device.R/AR/ARMedian/Sphere}",
            SortOrder: 7,
            IsEnabled: true,
            Description: null));

        var rule = Assert.Single(_adapter.Adapt(profile));

        Assert.Equal("6228", rule.TargetFieldCode);
        Assert.Equal(string.Empty, rule.SourcePath);
        Assert.Equal("R.:S={Device.R/AR/ARMedian/Sphere}", rule.OutputTemplate);
        Assert.Equal(7, rule.SortOrder);
    }

    [Fact]
    public void Adapt_ShouldKeepDisabledRuleDisabled()
    {
        var profile = CreateExportProfile(new ExportRuleDefinition(
            Id: "disabled-1",
            TargetFieldCode: "6228",
            TargetName: "Disabled",
            RuleType: ExportRuleType.Template,
            SourcePath: null,
            OutputTemplate: "disabled",
            SortOrder: 99,
            IsEnabled: false,
            Description: null));

        var rule = Assert.Single(_adapter.Adapt(profile));

        Assert.False(rule.IsEnabled);
        Assert.Equal(99, rule.SortOrder);
    }

    private static ExportProfileDefinition CreateExportProfile(ExportRuleDefinition rule)
    {
        return DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Rules = new[] { rule }
        };
    }
}
