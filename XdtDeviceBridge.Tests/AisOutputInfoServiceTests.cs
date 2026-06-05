using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class AisOutputInfoServiceTests
{
    private readonly AisOutputInfoService _service = new();

    [Fact]
    public void Create_ShouldDescribeAisOutputFieldsWithoutValuesOrSourcePaths()
    {
        var catalog = CreateCatalog(
            DefaultDeviceProfileDefinitions.CreateNidekLm7Default(),
            DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm7Default());
        var interfaceProfile = catalog.InterfaceProfiles.Single();

        var info = _service.Create(catalog, interfaceProfile);

        Assert.Equal("LENS", info.DefaultExaminationType);
        Assert.Contains(info.Fields, field => field.FieldCode == "8402" && !field.IsCardField);
        Assert.Contains(info.Fields, field => field.FieldCode == "6228" && field.IsCardField);
        Assert.DoesNotContain(info.Fields, field => field.Hint.Contains("SourcePath", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(info.Fields, field => field.Hint.Contains("{value}", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Create_ShouldMarkPureDocumentAttachmentFieldsAsCardRelevant()
    {
        var catalog = CreateCatalog(
            DefaultDeviceProfileDefinitions.CreateDocumentAttachmentDefault(),
            DefaultExportProfileDefinitions.CreateMedistarDocumentAttachmentDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarDocumentAttachmentDefault());
        var interfaceProfile = catalog.InterfaceProfiles.Single();

        var info = _service.Create(catalog, interfaceProfile);

        Assert.Equal("DOKU", info.DefaultExaminationType);
        Assert.Contains(info.Fields, field => field.FieldCode == "6302" && field.IsCardField && field.CardVisibility == "Ja");
        Assert.Contains(info.Fields, field => field.FieldCode == "6303" && field.IsCardField && field.CardVisibility == "Ja");
        Assert.Contains(info.Fields, field => field.FieldCode == "6305" && field.IsCardField && field.CardVisibility == "Ja");
    }

    [Fact]
    public void Create_ShouldMarkManualDocumentTransferAttachmentFieldsAsCardRelevant()
    {
        var catalog = CreateCatalog(
            DefaultDeviceProfileDefinitions.CreateManualDocumentSelectionDefault(),
            DefaultExportProfileDefinitions.CreateMedistarManualDocumentTransferDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarManualDocumentTransferDefault());
        var interfaceProfile = catalog.InterfaceProfiles.Single();

        var info = _service.Create(catalog, interfaceProfile);

        Assert.Equal("DOKU", info.DefaultExaminationType);
        Assert.Contains(info.Fields, field => field.FieldCode == "6302" && field.IsCardField && field.CardVisibility == "Ja");
        Assert.Contains(info.Fields, field => field.FieldCode == "6303" && field.IsCardField && field.CardVisibility == "Ja");
        Assert.Contains(info.Fields, field => field.FieldCode == "6305" && field.IsCardField && field.CardVisibility == "Ja");
    }

    [Fact]
    public void Create_ShouldKeepAttachmentFieldsOptionalForMeasuringDevices()
    {
        var catalog = CreateCatalog(
            DefaultDeviceProfileDefinitions.CreateNidekLm7Default(),
            DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm7Default());
        var interfaceProfile = catalog.InterfaceProfiles.Single() with
        {
            FolderOptions = catalog.InterfaceProfiles.Single().FolderOptions with
            {
                IsAttachmentProcessingEnabled = true,
                AttachmentExportFolder = @"C:\XDT\Anhang"
            }
        };
        catalog = catalog with { InterfaceProfiles = new[] { interfaceProfile } };

        var info = _service.Create(catalog, interfaceProfile);

        Assert.Contains(info.Fields, field => field.FieldCode == "6302" && field.IsOptional);
        Assert.Contains(info.Fields, field => field.FieldCode == "6303" && field.IsOptional);
        Assert.Contains(info.Fields, field => field.FieldCode == "6305" && field.IsOptional);
    }

    [Fact]
    public void Create_ShouldDescribePhoropterCardFields()
    {
        var catalog = CreateCatalog(
            DefaultDeviceProfileDefinitions.CreateNidekRt3100SerialDefault(),
            DefaultExportProfileDefinitions.CreateMedistarNidekRt3100SerialDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt3100SerialDefault());
        var interfaceProfile = catalog.InterfaceProfiles.Single();

        var info = _service.Create(catalog, interfaceProfile);

        Assert.Equal("PHORO", info.DefaultExaminationType);
        Assert.Contains(info.Fields, field => field.FieldCode == "6228"
            && field.CardVisibility == "Ja"
            && field.Meaning.Contains("Phoropter", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("abweichende Untersuchungsarten", info.ExaminationTypeHint, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Lensmeter", "LM-7", "Xml", "LENS")]
    [InlineData("Phoropter", "RT-3100", "NidekRtSerialPhoropter", "PHORO")]
    [InlineData("Autorefraktor", "ARK-1s", "Xml", "AUTO")]
    [InlineData("Keratometer", "KM-1", "Xml", "KERA")]
    [InlineData("Keratorefraktometer", "TRK-2P", "Xml", "KOMB")]
    [InlineData("Tonometer", "NT-530P", "Xml", "TONO")]
    [InlineData("Pachymeter", "CCT", "Xml", "PACHY")]
    [InlineData("Endothel", "EM-4000", "Xml", "ENDO")]
    [InlineData("OCT", "OCT", "Xml", "OCT")]
    [InlineData("Dokument", "Dokument", "Manual", "DOKU")]
    [InlineData("Sonstiges", "Unbekannt", "Text", "MESS")]
    public void ResolveExaminationTypeDefault_ShouldReturnRecommendedValue(
        string deviceType,
        string model,
        string parserMode,
        string expected)
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekLm7Default() with
        {
            Metadata = DefaultDeviceProfileDefinitions.CreateNidekLm7Default().Metadata with
            {
                Name = model,
                Product = model
            },
            DeviceType = deviceType,
            Model = model,
            ParserMode = parserMode,
            SupportedExaminationTypes = Array.Empty<string>()
        };

        Assert.Equal(expected, AisExaminationTypeDefaults.Resolve(profile));
    }

    [Fact]
    public void ResolveExaminationTypeDefault_ShouldNotClassifyBuiltInAutorefractorsAsPhoropter()
    {
        var profiles = GetBuiltInDeviceProfiles();
        var autorefractors = profiles.Where(profile =>
            string.Join(' ', profile.Metadata.Name, profile.DeviceType, profile.Model, profile.ParserMode)
                .Contains("autorefr", StringComparison.OrdinalIgnoreCase)
            || string.Join(' ', profile.Metadata.Name, profile.DeviceType, profile.Model, profile.ParserMode)
                .Contains("ARK", StringComparison.OrdinalIgnoreCase)
            || string.Join(' ', profile.Metadata.Name, profile.DeviceType, profile.Model, profile.ParserMode)
                .Contains(" AR-", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.NotEmpty(autorefractors);
        Assert.DoesNotContain(autorefractors, profile =>
            AisExaminationTypeDefaults.Resolve(profile) == "PHORO");
    }

    private static IReadOnlyList<DeviceProfileDefinition> GetBuiltInDeviceProfiles()
    {
        return typeof(DefaultDeviceProfileDefinitions)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(method => method.GetParameters().Length == 0
                && method.ReturnType == typeof(DeviceProfileDefinition))
            .Select(method => (DeviceProfileDefinition)method.Invoke(null, null)!)
            .ToArray();
    }

    private static ProfileCatalog CreateCatalog(
        DeviceProfileDefinition deviceProfile,
        ExportProfileDefinition exportProfile,
        InterfaceProfileDefinition interfaceProfile)
    {
        return new ProfileCatalog(
            new[] { DefaultAisProfiles.CreateMedistarDefault() },
            new[] { deviceProfile },
            new[] { exportProfile },
            new[] { interfaceProfile });
    }
}
