using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class XdtBaukastenDeviceOutputRuleServiceTests
{
    private readonly MedistarHistoricalMeasurementParser _historyParser = new();
    private readonly TopconCv5000ImportXmlWriter _cv5000Writer = new();
    private readonly NidekRt6100InputXmlWriter _rt6100Writer = new();

    [Fact]
    public void CreateDefaultRules_ShouldInitializeCv5000DeviceOutputRules()
    {
        var rules = XdtBaukastenDeviceOutputRuleService.CreateDefaultRules(DefaultDeviceProfileDefinitions.CreateTopconCv5000Default());

        Assert.Contains(rules, rule => rule.TargetFieldCode == "Common/Patient/ID" && rule.SourcePath == "CV5000Input.Patient.ID");
        Assert.Contains(rules, rule => rule.TargetFieldCode == "SBJ/Lensmeter/R/Sph" && rule.SourcePath == "PhoropterInput.Lensmeter.Right.Sphere");
        Assert.Contains(rules, rule => rule.TargetFieldCode == "SBJ/Autorefraction/L/Cyl" && rule.SourcePath == "PhoropterInput.Autoref.Left.Cylinder");
        Assert.True(rules.Count > 20);
    }

    [Fact]
    public void CreateDefaultRules_ShouldInitializeRt6100DeviceOutputRules()
    {
        var rules = XdtBaukastenDeviceOutputRuleService.CreateDefaultRules(DefaultDeviceProfileDefinitions.CreateNidekRt6100Default());

        Assert.Contains(rules, rule => rule.TargetFieldCode == "Common/Patient/ID" && rule.SourcePath == "RT6100Input.Patient.ID");
        Assert.Contains(rules, rule => rule.TargetFieldCode == "RT/LM_Base/R/Sphere" && rule.SourcePath == "PhoropterInput.LM_Base.Right.Sphere");
        Assert.Contains(rules, rule => rule.TargetFieldCode == "RT/REF_Base/L/Cylinder" && rule.SourcePath == "PhoropterInput.REF_Base.Left.Cylinder");
        Assert.True(rules.Count > 15);
    }

    [Fact]
    public void CreatePlaceholders_ShouldExposeExpandedCv5000Values()
    {
        var history = ParseCv5000History();

        var placeholders = XdtBaukastenDeviceOutputRuleService.CreatePlaceholders(
            DefaultDeviceProfileDefinitions.CreateTopconCv5000Default(),
            history.Patient,
            history.Records);

        Assert.Contains(placeholders, placeholder => placeholder.Token == "{CV5000Input.PatientNumber}" && placeholder.ExampleValue == "4701-1");
        Assert.Contains(placeholders, placeholder => placeholder.Token == "{PhoropterInput.Lensmeter.Right.Sphere}" && placeholder.ExampleValue == "+6.25");
        Assert.Contains(placeholders, placeholder => placeholder.Token == "{PhoropterInput.Autoref.Left.Cylinder}" && placeholder.ExampleValue != "-");
        Assert.True(placeholders.Count > 10);
    }

    [Fact]
    public void ApplyRulesToXml_ShouldProjectCv5000RuleChangesIntoPreview()
    {
        var history = ParseCv5000History();
        var selected = _historyParser.CreateDefaultCv5000Selection(history.Records);
        var xml = _cv5000Writer.BuildXml(new Cv5000ImportSelection(history.Patient, selected, null, "CVImport.xml")).XmlContent!;
        var rules = XdtBaukastenDeviceOutputRuleService.CreateDefaultRules(DefaultDeviceProfileDefinitions.CreateTopconCv5000Default())
            .Select(rule => rule.TargetFieldCode == "Common/Patient/ID"
                ? rule with { OutputTemplate = "BAU-ID" }
                : rule)
            .ToArray();

        var result = XdtBaukastenDeviceOutputRuleService.ApplyRulesToXml(
            xml,
            rules,
            history.Patient,
            selected,
            DefaultDeviceProfileDefinitions.CreateTopconCv5000Default());

        Assert.Empty(result.Warnings);
        Assert.Contains("BAU-ID", result.Content);
        Assert.DoesNotContain("<nsCommon:ID>4701-1</nsCommon:ID>", result.Content);
    }

    [Fact]
    public void ApplyRulesToXml_ShouldRemoveDeletedCv5000DefaultTargetFromPreview()
    {
        var history = ParseCv5000History();
        var selected = _historyParser.CreateDefaultCv5000Selection(history.Records);
        var xml = _cv5000Writer.BuildXml(new Cv5000ImportSelection(history.Patient, selected, null, "CVImport.xml")).XmlContent!;
        var rules = XdtBaukastenDeviceOutputRuleService.CreateDefaultRules(DefaultDeviceProfileDefinitions.CreateTopconCv5000Default())
            .Where(rule => rule.TargetFieldCode != "Common/Patient/ID")
            .ToArray();

        var result = XdtBaukastenDeviceOutputRuleService.ApplyRulesToXml(
            xml,
            rules,
            history.Patient,
            selected,
            DefaultDeviceProfileDefinitions.CreateTopconCv5000Default());

        Assert.DoesNotContain("<nsCommon:ID>", result.Content);
    }

    [Fact]
    public void ApplyRulesToXml_ShouldProjectRt6100RuleChangesIntoPreview()
    {
        var history = ParseCv5000History();
        var selected = _historyParser.CreateDefaultRt6100Selection(history.Records);
        var xml = _rt6100Writer.BuildXml(new Cv5000ImportSelection(history.Patient, selected, null, NidekRt6100InputXmlWriter.DefaultFileNameTemplate)).XmlContent!;
        var rules = XdtBaukastenDeviceOutputRuleService.CreateDefaultRules(DefaultDeviceProfileDefinitions.CreateNidekRt6100Default())
            .Select(rule => rule.TargetFieldCode == "Common/Patient/ID"
                ? rule with { OutputTemplate = "RT-ID" }
                : rule)
            .ToArray();

        var result = XdtBaukastenDeviceOutputRuleService.ApplyRulesToXml(
            xml,
            rules,
            history.Patient,
            selected,
            DefaultDeviceProfileDefinitions.CreateNidekRt6100Default());

        Assert.Empty(result.Warnings);
        Assert.Contains("<ID>RT-ID</ID>", result.Content);
    }

    [Fact]
    public void UserDefinedBidirectionalDevice_ShouldStayOpenWithoutDefaultRules()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekRt6100Default() with
        {
            Metadata = DefaultDeviceProfileDefinitions.CreateNidekRt6100Default().Metadata with
            {
                Id = "device-userdefined-bidi",
                Name = "Eigenes bidirektionales Gerät",
                IsBuiltIn = false,
                IsUserDefined = true
            },
            Model = "Eigenes Gerät"
        };

        var rules = XdtBaukastenDeviceOutputRuleService.CreateDefaultRules(profile);
        var placeholders = XdtBaukastenDeviceOutputRuleService.CreatePlaceholders(profile, null, Array.Empty<AisHistoricalMeasurementRecord>());

        Assert.Empty(rules);
        Assert.Contains(placeholders, placeholder => placeholder.DisplayName == "Geräteausgabe vorbereitet");
    }

    private MedistarHistoricalMeasurementParseResult ParseCv5000History()
    {
        return _historyParser.ParseFile(Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "Devices",
            "Topcon",
            "CV5000",
            "Patient_mit_Phoropter_Daten.XDT"));
    }
}
