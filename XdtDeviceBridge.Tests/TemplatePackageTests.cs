using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class TemplatePackageTests
{
    [Fact]
    public void Validate_ShouldAcceptValidTemplatePackage()
    {
        var package = CreateValidPackage();

        var issues = TemplatePackageValidator.Validate(package);

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ShouldReportMissingPackageFormatVersion()
    {
        var package = CreateValidPackage() with { PackageFormatVersion = "" };

        var issues = TemplatePackageValidator.Validate(package);

        Assert.Contains("PackageFormatVersion must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportWrongProfileKind()
    {
        var package = CreateValidPackage() with
        {
            Metadata = CreateMetadata("wrong-kind", "Wrong Kind", ProfileKind.InterfaceProfile)
        };

        var issues = TemplatePackageValidator.Validate(package);

        Assert.Contains("Metadata.ProfileKind must be TemplatePackage.", issues);
    }

    [Fact]
    public void Validate_ShouldReportDuplicateIncludedProfileIds()
    {
        var package = CreateValidPackage() with
        {
            IncludedProfiles = new[]
            {
                CreateMetadata("profile-1", "Profile 1", ProfileKind.AisProfile),
                CreateMetadata("PROFILE-1", "Profile 1 Copy", ProfileKind.DeviceProfile)
            }
        };

        var issues = TemplatePackageValidator.Validate(package);

        Assert.Contains("IncludedProfiles contains duplicate Id: profile-1", issues);
    }

    [Fact]
    public void Validate_ShouldAllowEmptyIncludedProfiles()
    {
        var package = CreateValidPackage() with { IncludedProfiles = Array.Empty<ProfileMetadata>() };

        var issues = TemplatePackageValidator.Validate(package);

        Assert.Empty(issues);
    }

    private static TemplatePackage CreateValidPackage()
    {
        return new TemplatePackage(
            Metadata: CreateMetadata("package-1", "MEDISTAR + NIDEK ARK1S", ProfileKind.TemplatePackage),
            IncludedProfiles: new[]
            {
                CreateMetadata("ais-medistar", "MEDISTAR", ProfileKind.AisProfile),
                CreateMetadata("device-nidek-ark1s", "NIDEK ARK1S", ProfileKind.DeviceProfile),
                CreateMetadata("export-medistar-ark1s", "MEDISTAR ARK1S Export", ProfileKind.ExportProfile)
            },
            PackageFormatVersion: "1.0",
            CreatedAt: new DateTime(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc),
            CreatedBy: "XdtDeviceBridge",
            Description: "Validated package model test fixture.");
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
            IsBuiltIn: false,
            IsUserDefined: true);
    }
}
