using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class ProfileMetadataTests
{
    [Fact]
    public void ProfileMetadata_ShouldBeCreated()
    {
        var metadata = CreateValidMetadata();

        Assert.Equal("medistar-nidek-ark1s", metadata.Id);
        Assert.Equal("MEDISTAR + NIDEK ARK1S", metadata.Name);
        Assert.Equal(ProfileKind.InterfaceProfile, metadata.ProfileKind);
        Assert.Equal("1.0.0", metadata.Version);
    }

    [Fact]
    public void ProfileMetadata_ShouldStoreBuiltInAndUserDefinedFlags()
    {
        var metadata = CreateValidMetadata() with
        {
            IsBuiltIn = true,
            IsUserDefined = false
        };

        Assert.True(metadata.IsBuiltIn);
        Assert.False(metadata.IsUserDefined);
    }

    [Fact]
    public void Validate_ShouldAcceptValidMetadata()
    {
        var issues = ProfileMetadataValidator.Validate(CreateValidMetadata());

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyId()
    {
        var metadata = CreateValidMetadata() with { Id = "" };

        var issues = ProfileMetadataValidator.Validate(metadata);

        Assert.Contains("Id must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyName()
    {
        var metadata = CreateValidMetadata() with { Name = " " };

        var issues = ProfileMetadataValidator.Validate(metadata);

        Assert.Contains("Name must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyVersion()
    {
        var metadata = CreateValidMetadata() with { Version = "" };

        var issues = ProfileMetadataValidator.Validate(metadata);

        Assert.Contains("Version must not be empty.", issues);
    }

    private static ProfileMetadata CreateValidMetadata()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new ProfileMetadata(
            Id: "medistar-nidek-ark1s",
            Name: "MEDISTAR + NIDEK ARK1S",
            ProfileKind: ProfileKind.InterfaceProfile,
            Description: "Validated first interface profile.",
            Vendor: "NIDEK",
            Product: "ARK1S",
            Version: "1.0.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "XdtDeviceBridge",
            IsBuiltIn: true,
            IsUserDefined: false);
    }
}
