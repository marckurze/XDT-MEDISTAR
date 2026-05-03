using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TemplatePackageImportValidatorTests
{
    private readonly TemplatePackageImportValidator _validator = new();

    [Fact]
    public void Validate_ShouldAcceptValidImportResult()
    {
        var result = _validator.Validate(CreateValidResult());

        Assert.False(result.HasErrors);
        Assert.False(result.HasWarnings);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Validate_ShouldReportMissingPackage()
    {
        var importResult = CreateValidResult() with { Package = null! };

        var result = _validator.Validate(importResult);

        AssertError(result, "Package must not be null.");
    }

    [Fact]
    public void Validate_ShouldReportWrongPackageProfileKind()
    {
        var package = CreatePackage(Array.Empty<ProfileMetadata>()) with
        {
            Metadata = CreateMetadata("package-wrong-kind", "Wrong Kind", ProfileKind.InterfaceProfile)
        };
        var importResult = CreateValidResult() with { Package = package };

        var result = _validator.Validate(importResult);

        AssertError(result, "Package Metadata.ProfileKind must be TemplatePackage.");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateAisProfileId()
    {
        var aisProfile = DefaultAisProfiles.CreateMedistarDefault();
        var duplicate = aisProfile with { Name = "MEDISTAR Copy" };
        var importResult = CreateValidResult() with { AisProfiles = new[] { aisProfile, duplicate } };

        var result = _validator.Validate(importResult);

        AssertError(result, "Duplicate AisProfile profile Id: ais-medistar-default");
    }

    [Fact]
    public void Validate_ShouldReportInterfaceMissingAisReference()
    {
        var importResult = CreateValidResult() with { AisProfiles = Array.Empty<AisProfile>() };

        var result = _validator.Validate(importResult);

        AssertError(result, "Interface profile references missing AIS profile: ais-medistar-default");
    }

    [Fact]
    public void Validate_ShouldReportInterfaceMissingDeviceReference()
    {
        var importResult = CreateValidResult() with { DeviceProfiles = Array.Empty<DeviceProfileDefinition>() };

        var result = _validator.Validate(importResult);

        AssertError(result, "Interface profile references missing Device profile: device-nidek-ark1s-default");
    }

    [Fact]
    public void Validate_ShouldReportInterfaceMissingExportReference()
    {
        var importResult = CreateValidResult() with { ExportProfiles = Array.Empty<ExportProfileDefinition>() };

        var result = _validator.Validate(importResult);

        AssertError(result, "Interface profile references missing Export profile: export-medistar-nidek-ark1s-default");
    }

    [Fact]
    public void Validate_ShouldReportExportMissingAisReference()
    {
        var importResult = CreateValidResult() with { AisProfiles = Array.Empty<AisProfile>() };

        var result = _validator.Validate(importResult);

        AssertError(result, "Export profile references missing AIS profile: ais-medistar-default");
    }

    [Fact]
    public void Validate_ShouldReportExportMissingDeviceReference()
    {
        var importResult = CreateValidResult() with { DeviceProfiles = Array.Empty<DeviceProfileDefinition>() };

        var result = _validator.Validate(importResult);

        AssertError(result, "Export profile references missing Device profile: device-nidek-ark1s-default");
    }

    [Fact]
    public void Validate_ShouldReportActiveInterfaceWithoutFolders()
    {
        var activeInterface = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            IsActive = true
        };
        var importResult = CreateValidResult() with { InterfaceProfiles = new[] { activeInterface } };

        var result = _validator.Validate(importResult);

        AssertError(result, "Active interface profile requires AisImportFolder.");
        AssertError(result, "Active interface profile requires DeviceImportFolder.");
        AssertError(result, "Active interface profile requires ExportFolder.");
    }

    [Fact]
    public void Validate_ShouldWarnWhenDeleteOptionIsEnabled()
    {
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            FolderOptions = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions with
            {
                ClearAisImportFolderBeforeProcessing = true
            }
        };
        var importResult = CreateValidResult() with { InterfaceProfiles = new[] { interfaceProfile } };

        var result = _validator.Validate(importResult);

        Assert.False(result.HasErrors);
        AssertWarning(result, "Imported delete options must be reviewed before productive activation.");
    }

    [Fact]
    public void Validate_ShouldWarnWhenInterfaceProfilesAreMissing()
    {
        var importResult = CreateValidResult() with { InterfaceProfiles = Array.Empty<InterfaceProfileDefinition>() };

        var result = _validator.Validate(importResult);

        Assert.False(result.HasErrors);
        AssertWarning(result, "Template package does not contain any interface profiles.");
    }

    private static TemplatePackageImportResult CreateValidResult()
    {
        var aisProfile = DefaultAisProfiles.CreateMedistarDefault();
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();

        var package = CreatePackage(new[]
        {
            aisProfile.Metadata,
            deviceProfile.Metadata,
            exportProfile.Metadata,
            interfaceProfile.Metadata
        });

        return new TemplatePackageImportResult(
            Package: package,
            AisProfiles: new[] { aisProfile },
            DeviceProfiles: new[] { deviceProfile },
            ExportProfiles: new[] { exportProfile },
            InterfaceProfiles: new[] { interfaceProfile });
    }

    private static TemplatePackage CreatePackage(IReadOnlyList<ProfileMetadata> includedProfiles)
    {
        return new TemplatePackage(
            Metadata: CreateMetadata("package-medistar-nidek-ark1s", "MEDISTAR + NIDEK ARK1S Package", ProfileKind.TemplatePackage),
            IncludedProfiles: includedProfiles,
            PackageFormatVersion: "1.0",
            CreatedAt: new DateTime(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc),
            CreatedBy: "XdtDeviceBridge",
            Description: "Import validation test package.");
    }

    private static ProfileMetadata CreateMetadata(string id, string name, ProfileKind profileKind)
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new ProfileMetadata(
            Id: id,
            Name: name,
            ProfileKind: profileKind,
            Description: null,
            Vendor: null,
            Product: null,
            Version: "1.0.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "XdtDeviceBridge",
            IsBuiltIn: true,
            IsUserDefined: false);
    }

    private static void AssertError(TemplatePackageImportValidationResult result, string message)
    {
        Assert.True(result.HasErrors);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == TemplatePackageImportValidationIssueSeverity.Error
            && issue.Message == message);
    }

    private static void AssertWarning(TemplatePackageImportValidationResult result, string message)
    {
        Assert.True(result.HasWarnings);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == TemplatePackageImportValidationIssueSeverity.Warning
            && issue.Message == message);
    }
}
