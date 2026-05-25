using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class DeviceProfileImageOverrideServiceTests
{
    private readonly DeviceProfileImageOverrideService _service = new();

    [Fact]
    public void SaveImageOverride_ShouldCopyImageToLocalDeviceImagesFolder()
    {
        var paths = CreateAppDataPaths();
        var sourceImage = CreateSourceImage(paths, "quelle.png", "image-content");

        var result = _service.SaveImageOverride(paths, "device-topcon-cv5000-default", sourceImage);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.NotNull(result.ImagePath);
        Assert.True(File.Exists(result.ImagePath));
        Assert.StartsWith(_service.GetDeviceImagesFolder(paths), result.ImagePath!, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("image-content", File.ReadAllText(result.ImagePath!));

        var overrides = _service.LoadOverrides(paths);
        Assert.Equal(result.ImagePath, overrides["device-topcon-cv5000-default"]);
    }

    [Fact]
    public void SaveImageOverride_ShouldRejectUnsupportedExtensions()
    {
        var paths = CreateAppDataPaths();
        var sourceImage = CreateSourceImage(paths, "quelle.gif", "image-content");

        var result = _service.SaveImageOverride(paths, "device-topcon-cv5000-default", sourceImage);

        Assert.False(result.Success);
        Assert.Contains("PNG", result.ErrorMessage);
        Assert.Empty(_service.LoadOverrides(paths));
    }

    [Fact]
    public void RemoveImageOverride_ShouldRemoveMappingAndLocalFile()
    {
        var paths = CreateAppDataPaths();
        var sourceImage = CreateSourceImage(paths, "quelle.jpg", "image-content");
        var saved = _service.SaveImageOverride(paths, "device-topcon-cv5000-default", sourceImage);
        Assert.True(File.Exists(saved.ImagePath));

        _service.RemoveImageOverride(paths, "device-topcon-cv5000-default");

        Assert.Empty(_service.LoadOverrides(paths));
        Assert.False(File.Exists(saved.ImagePath!));
    }

    [Fact]
    public void BuildManagementItems_ShouldListBuiltInAndUserDefinedDevices()
    {
        var paths = CreateAppDataPaths();
        var builtIn = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default();
        var userDefined = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() with
        {
            Metadata = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault().Metadata with
            {
                Id = "device-userdefined-room-1",
                Name = "ARK1S Raum 1",
                IsBuiltIn = false,
                IsUserDefined = true
            }
        };

        var items = _service.BuildManagementItems(paths, new[] { builtIn, userDefined });

        var builtInItem = Assert.Single(items, item => item.DeviceProfileId == builtIn.Metadata.Id);
        Assert.True(builtInItem.IsBuiltIn);
        Assert.Equal("BuiltIn", builtInItem.ProfileKindDisplay);
        Assert.Equal("TOPCON", builtInItem.Manufacturer);
        Assert.Equal("CV-5000 / CV-5000S", builtInItem.Model);

        var userDefinedItem = Assert.Single(items, item => item.DeviceProfileId == userDefined.Metadata.Id);
        Assert.False(userDefinedItem.IsBuiltIn);
        Assert.Equal("UserDefined", userDefinedItem.ProfileKindDisplay);
        Assert.Equal("ARK1S Raum 1", userDefinedItem.ProfileName);
    }

    [Fact]
    public void BuildManagementItems_ShouldResolveCv5000BuiltInImageWhenPersistedProfileHasNoImagePath()
    {
        var paths = CreateAppDataPaths();
        var builtInWithoutImagePath = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default() with
        {
            DeviceImagePath = string.Empty
        };

        var item = Assert.Single(_service.BuildManagementItems(paths, new[] { builtInWithoutImagePath }));

        Assert.Equal(InterfaceProfileUiPolicy.TopconCv5000DeviceImagePath, item.EffectiveImagePath);
        Assert.False(item.HasLocalOverride);
    }

    [Fact]
    public void ResolveEffectiveImagePath_ShouldPreferExistingOverride()
    {
        var paths = CreateAppDataPaths();
        var sourceImage = CreateSourceImage(paths, "override.jpeg", "image-content");
        var saved = _service.SaveImageOverride(paths, "device-topcon-cv5000-default", sourceImage);
        var profile = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default();

        var resolved = _service.ResolveEffectiveImagePath(profile, saved.ImagePath);

        Assert.Equal(saved.ImagePath, resolved);
    }

    [Fact]
    public void ResolveEffectiveImagePath_ShouldFallBackToProfileImageWhenOverrideFileIsMissing()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default();

        var resolved = _service.ResolveEffectiveImagePath(profile, @"C:\nicht-vorhanden\bild.png");

        Assert.Equal(profile.DeviceImagePath, resolved);
    }

    [Fact]
    public void ResolveEffectiveImagePath_ShouldFallBackToCv5000BuiltInImageWhenProfileImageIsMissing()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default() with
        {
            DeviceImagePath = string.Empty
        };

        var resolved = _service.ResolveEffectiveImagePath(profile, overridePath: null);

        Assert.Equal(InterfaceProfileUiPolicy.TopconCv5000DeviceImagePath, resolved);
    }

    private static AppDataPaths CreateAppDataPaths()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return new AppDataPathProvider().GetPaths(baseFolder);
    }

    private static string CreateSourceImage(AppDataPaths paths, string fileName, string content)
    {
        var sourceFolder = Path.Combine(paths.BaseFolder, "source-images");
        Directory.CreateDirectory(sourceFolder);
        var sourcePath = Path.Combine(sourceFolder, fileName);
        File.WriteAllText(sourcePath, content);
        return sourcePath;
    }
}
