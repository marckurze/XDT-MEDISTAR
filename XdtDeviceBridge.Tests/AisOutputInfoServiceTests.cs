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
            DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault(),
            DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekNt530PDefault());
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

        Assert.Equal("TONO", info.DefaultExaminationType);
        Assert.Contains(info.Fields, field => field.FieldCode == "6302" && field.IsOptional);
        Assert.Contains(info.Fields, field => field.FieldCode == "6303" && field.IsOptional);
        Assert.Contains(info.Fields, field => field.FieldCode == "6305" && field.IsOptional);
    }

    [Fact]
    public void Create_ShouldShowTonoForNidekNt1E()
    {
        var catalog = CreateCatalog(
            DefaultDeviceProfileDefinitions.CreateNidekNt1EDefault(),
            DefaultExportProfileDefinitions.CreateMedistarNidekNt1EDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekNt1EDefault());
        var interfaceProfile = catalog.InterfaceProfiles.Single();

        var info = _service.Create(catalog, interfaceProfile);

        Assert.Equal("NIDEK NT-1E", info.DeviceProfileName);
        Assert.Equal("Tonometer/Pachymeter", info.DeviceType);
        Assert.Equal("TONO", info.DefaultExaminationType);
        Assert.Contains(info.Fields, field =>
            field.FieldCode == "8402"
            && field.Hint.Contains("TONO", StringComparison.Ordinal));
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
    [InlineData("Biometrie/IOL/Keratometer", "IOLMaster 700", "ZeissIolMaster700", "IOL")]
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

    [Theory]
    [MemberData(nameof(TonometerBuiltIns))]
    public void ResolveExaminationTypeDefault_ShouldReturnTonoForBuiltInTonometers(DeviceProfileDefinition profile)
    {
        Assert.Equal("TONO", AisExaminationTypeDefaults.Resolve(profile));
    }

    [Theory]
    [MemberData(nameof(RepresentativeBuiltInExamTypeExpectations))]
    public void ResolveExaminationTypeDefault_ShouldMatchRepresentativeBuiltInDeviceTypes(
        DeviceProfileDefinition profile,
        string expected)
    {
        Assert.Equal(expected, AisExaminationTypeDefaults.Resolve(profile));
    }

    [Fact]
    public void ResolveExaminationTypeDefault_ShouldNotTreatOptionalAttachmentsAsDocumentDevices()
    {
        var measuringProfilesWithAttachments = GetBuiltInDeviceProfiles()
            .Where(profile => profile.Metadata.Id is not "device-document-attachment-default"
                and not "device-manual-document-selection-default")
            .Where(profile => profile.SupportedExaminationTypes.Any(value =>
                    value.Contains("Attachment", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Anhang", StringComparison.OrdinalIgnoreCase))
                || profile.Measurements.Any(measurement =>
                    measurement.Group.Contains("Attachment", StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        Assert.NotEmpty(measuringProfilesWithAttachments);
        Assert.DoesNotContain(measuringProfilesWithAttachments, profile =>
            AisExaminationTypeDefaults.Resolve(profile) == "DOKU");
    }

    [Fact]
    public void ResolveExaminationTypeDefault_ShouldClassifyAllBuiltInsByDeviceIdentity()
    {
        var allowedDefaults = new HashSet<string>(StringComparer.Ordinal)
        {
            "LENS",
            "PHORO",
            "AUTO",
            "KERA",
            "KOMB",
            "TONO",
            "PACHY",
            "ENDO",
            "IOL",
            "OCT",
            "DOKU",
            "MESS"
        };
        var profiles = GetBuiltInDeviceProfiles();

        Assert.NotEmpty(profiles);
        foreach (var profile in profiles)
        {
            var actual = AisExaminationTypeDefaults.Resolve(profile);

            Assert.Contains(actual, allowedDefaults);
            Assert.Equal(ResolveExpectedBuiltInDefault(profile), actual);
        }
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

    public static IEnumerable<object[]> TonometerBuiltIns()
    {
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateNidekNt1Default() };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateNidekNt1EDefault() };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateNidekNt1PDefault() };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateNidekNt510Default() };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateNidekNt530Default() };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault() };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateTopconCt1PDefault() };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateTopconCt800ADefault() };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateHuvitzHnt1PDefault() };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateReichert7CrNctDefault() };
    }

    public static IEnumerable<object[]> RepresentativeBuiltInExamTypeExpectations()
    {
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateNidekLm7Default(), "LENS" };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateNidekRt3100SerialDefault(), "PHORO" };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault(), "AUTO" };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault(), "KOMB" };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateHuvitzHtr1ADefault(), "KOMB" };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateTopconCt1PDefault(), "TONO" };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateTomeyEm3000Default(), "ENDO" };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateZeissIolMaster700Default(), "IOL" };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateDocumentAttachmentDefault(), "DOKU" };
        yield return new object[] { DefaultDeviceProfileDefinitions.CreateManualDocumentSelectionDefault(), "DOKU" };
    }

    private static string ResolveExpectedBuiltInDefault(DeviceProfileDefinition profile)
    {
        var identity = string.Join(
            ' ',
            profile.Metadata.Name,
            profile.Metadata.Product ?? string.Empty,
            profile.Manufacturer,
            profile.DeviceType,
            profile.Model,
            profile.ParserMode);
        var supported = string.Join(' ', profile.SupportedExaminationTypes ?? Array.Empty<string>());
        var text = string.Join(' ', identity, supported);

        if (ContainsAny(identity, "document", "dokument", "attachmentonly", "attachment only", "anhang", "dokumentauswahl", "dokumentübergabe"))
        {
            return "DOKU";
        }

        if (ContainsAny(text, "oct"))
        {
            return "OCT";
        }

        if (ContainsAny(text, "endo", "em-"))
        {
            return "ENDO";
        }

        if (ContainsAny(text, "iolmaster", "iol master", "biometrie", "biometry", "achslaenge", "achsl", "axial"))
        {
            return "IOL";
        }

        if (IsExpectedCombinationDevice(text))
        {
            return "KOMB";
        }

        if (ContainsAny(text, "tonometer", "nct", "nt-", "tm", "ct-"))
        {
            return "TONO";
        }

        if (ContainsAny(text, "pachy", "cct"))
        {
            return "PACHY";
        }

        if (ContainsAny(text, "kerato", "keratometer", "km"))
        {
            return ContainsAny(text, "ref", "refrak", "auto") ? "KOMB" : "KERA";
        }

        if (IsExpectedPhoropterDevice(text))
        {
            return "PHORO";
        }

        if (ContainsAny(text, "lens", "lensmeter", "scheitel", "lm"))
        {
            return "LENS";
        }

        if (ContainsAny(text, "autorefr", "refrak", "ark", "ar-"))
        {
            return "AUTO";
        }

        return "MESS";
    }

    private static bool IsExpectedCombinationDevice(string text)
    {
        if (ContainsAny(text, "combo", "kombi", "komb", "trk-", "tonoref", "ref/km", "ref-km"))
        {
            return true;
        }

        var hasRef = ContainsAny(text, "autorefr", "refrak", "ref", "ark", "ar-");
        var hasKerato = ContainsAny(text, "kerato", "keratometer", "km");
        var hasTono = ContainsAny(text, "tonometer", "nct", "nt-", "tm");
        var hasPachy = ContainsAny(text, "pachy", "cct");

        return (hasRef && hasKerato)
            || (hasRef && hasTono)
            || (hasRef && hasPachy)
            || (hasKerato && hasTono)
            || (hasKerato && hasPachy);
    }

    private static bool IsExpectedPhoropterDevice(string text)
    {
        if (text.Contains("autorefractor", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ContainsAny(text, "phoropter", "refractor", "rt-");
    }

    private static bool ContainsAny(string text, params string[] needles)
    {
        return needles.Any(needle => text.Contains(needle, StringComparison.OrdinalIgnoreCase));
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
