using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class ProfileCatalogServiceTests
{
    private readonly ProfileCatalogService _service = new();

    [Fact]
    public void Load_ShouldReturnEmptyListsWhenFoldersAreMissing()
    {
        var paths = CreateAppDataPaths();

        var catalog = _service.Load(paths);

        Assert.Empty(catalog.AisProfiles);
        Assert.Empty(catalog.DeviceProfiles);
        Assert.Empty(catalog.ExportProfiles);
        Assert.Empty(catalog.InterfaceProfiles);
    }

    [Fact]
    public void Save_ShouldStoreAllProfileTypes()
    {
        var paths = CreateAppDataPaths();

        _service.Save(paths, CreateDefaultCatalog());

        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "ais", "ais-medistar-default.json")));
        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "devices", "device-nidek-ark1s-default.json")));
        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "exports", "export-medistar-nidek-ark1s-default.json")));
        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "interfaces", "interface-medistar-nidek-ark1s-default.json")));
    }

    [Fact]
    public void Load_ShouldReadSavedProfiles()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, CreateDefaultCatalog());

        var catalog = _service.Load(paths);

        Assert.Single(catalog.AisProfiles);
        Assert.Single(catalog.DeviceProfiles);
        Assert.Single(catalog.ExportProfiles);
        Assert.Single(catalog.InterfaceProfiles);
        Assert.Equal("MEDISTAR", catalog.AisProfiles[0].Name);
        Assert.Equal("NIDEK", catalog.DeviceProfiles[0].Manufacturer);
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldCreateDefaultProfiles()
    {
        var paths = CreateAppDataPaths();

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        Assert.Single(catalog.AisProfiles);
        Assert.Equal(18, catalog.DeviceProfiles.Count);
        Assert.Equal(18, catalog.ExportProfiles.Count);
        Assert.Equal(18, catalog.InterfaceProfiles.Count);
        Assert.Equal("ais-medistar-default", catalog.AisProfiles[0].Metadata.Id);
        AssertExpectedDeviceDefaults(catalog);
        AssertExpectedExportDefaults(catalog);
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-nidek-ark1s-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-nidek-ar360-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-nidek-lm7-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-nidek-nt530p-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-nidek-rt6100-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-nidek-rt2100-serial-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-nidek-rt3100-serial-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-nidek-rt5100-serial-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-topcon-cl300-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-topcon-solos-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-topcon-kr800-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-topcon-kr1-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-topcon-trk2p-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-topcon-ct1p-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-topcon-ct800a-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-topcon-cv5000-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-document-attachment-default");
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-manual-document-transfer-default");
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldCreateAllExpectedDeviceProfileDefinitions()
    {
        var paths = CreateAppDataPaths();

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        AssertExpectedDeviceDefaults(catalog);
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldCreateAllExpectedExportProfileDefinitions()
    {
        var paths = CreateAppDataPaths();

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        AssertExpectedExportDefaults(catalog);
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldKeepMedistarAisProfileAvailable()
    {
        var paths = CreateAppDataPaths();

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        Assert.Contains(catalog.AisProfiles, profile =>
            profile.Metadata.Id == "ais-medistar-default"
            && profile.Name == "MEDISTAR");
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldNotOverwriteExistingProfiles()
    {
        var paths = CreateAppDataPaths();
        var customAisProfile = DefaultAisProfiles.CreateMedistarDefault() with
        {
            Name = "Custom MEDISTAR"
        };
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: new[] { customAisProfile },
            DeviceProfiles: new[] { DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault() with { DeviceType = "Custom Tonometer/Pachymeter" } },
            ExportProfiles: new[] { DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault() with { OutputEncoding = "Custom-Encoding" } },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        Assert.Contains(catalog.AisProfiles, profile => profile.Metadata.Id == "ais-medistar-default" && profile.Name == "Custom MEDISTAR");
        Assert.Contains(catalog.DeviceProfiles, profile => profile.Metadata.Id == "device-topcon-trk2p-default" && profile.DeviceType == "Custom Tonometer/Pachymeter");
        Assert.Contains(catalog.ExportProfiles, profile => profile.Metadata.Id == "export-medistar-topcon-trk2p-default" && profile.OutputEncoding == "Custom-Encoding");
        Assert.Equal(18, catalog.DeviceProfiles.Count);
        Assert.Equal(18, catalog.ExportProfiles.Count);
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldRepairBuiltInNidekRtSerialSendMode()
    {
        var paths = CreateAppDataPaths();
        var legacy = DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt3100SerialDefault() with
        {
            NidekRtSerialSendMode = null,
            NidekRtSerialOutputFrameVariant = null
        };
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: Array.Empty<ExportProfileDefinition>(),
            InterfaceProfiles: new[] { legacy }));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-nidek-rt3100-serial-default");
        Assert.True(profile.Metadata.IsBuiltIn);
        Assert.False(profile.Metadata.IsUserDefined);
        Assert.Equal(NidekRtSerialSendMode.DirectWriterFrame, profile.NidekRtSerialSendMode);
        Assert.Equal(NidekRtSerialOutputFrameVariant.FullSelectedData, profile.NidekRtSerialOutputFrameVariant);
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldNotRepairUserDefinedNidekRtSerialSendMode()
    {
        var paths = CreateAppDataPaths();
        var userDefined = DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt3100SerialDefault() with
        {
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt3100SerialDefault().Metadata with
            {
                IsBuiltIn = false,
                IsUserDefined = true
            },
            NidekRtSerialSendMode = NidekRtSerialSendMode.RsSdHandshake,
            NidekRtSerialOutputFrameVariant = NidekRtSerialOutputFrameVariant.LmOnlyWithoutAdd
        };
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: Array.Empty<ExportProfileDefinition>(),
            InterfaceProfiles: new[] { userDefined }));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-medistar-nidek-rt3100-serial-default");
        Assert.False(profile.Metadata.IsBuiltIn);
        Assert.True(profile.Metadata.IsUserDefined);
        Assert.Equal(NidekRtSerialSendMode.RsSdHandshake, profile.NidekRtSerialSendMode);
        Assert.Equal(NidekRtSerialOutputFrameVariant.LmOnlyWithoutAdd, profile.NidekRtSerialOutputFrameVariant);
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldRepairLegacyLm7BuiltInExportProfile()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: new[] { CreateLegacyLm7ExportProfile(isBuiltIn: true) },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.ExportProfiles, profile => profile.Metadata.Id == "export-medistar-nidek-lm7-default");
        Assert.True(profile.Metadata.IsBuiltIn);
        Assert.False(profile.Metadata.IsUserDefined);
        Assert.Contains(profile.Rules, rule => rule.SourcePath == "Device.Measure[@Type='LM']/LM/R/MedistarLine");
        Assert.Contains(profile.Rules, rule => rule.SourcePath == "Device.Measure[@Type='LM']/LM/L/MedistarLine");
        Assert.DoesNotContain(profile.Rules, rule => ContainsLegacyLm7MedianPath(rule.SourcePath));
        Assert.DoesNotContain(profile.Rules, rule => ContainsLegacyLm7MedianPath(rule.OutputTemplate));
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldNotRepairUserDefinedLm7ExportProfile()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: new[] { CreateLegacyLm7ExportProfile(isBuiltIn: false) },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.ExportProfiles, profile => profile.Metadata.Id == "export-medistar-nidek-lm7-default");
        Assert.False(profile.Metadata.IsBuiltIn);
        Assert.True(profile.Metadata.IsUserDefined);
        Assert.Contains(profile.Rules, rule => ContainsLegacyLm7MedianPath(rule.OutputTemplate));
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldRepairLegacyTopconCl300BuiltInDeviceProfile()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: new[] { CreateLegacyTopconCl300DeviceProfile(isBuiltIn: true) },
            ExportProfiles: Array.Empty<ExportProfileDefinition>(),
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.DeviceProfiles, profile => profile.Metadata.Id == "device-topcon-cl300-default");
        Assert.True(profile.Metadata.IsBuiltIn);
        Assert.False(profile.Metadata.IsUserDefined);
        Assert.Contains(profile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/R/MedistarLine");
        Assert.Contains(profile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/L/MedistarLine");
        Assert.DoesNotContain(profile.Measurements, measurement => ContainsLegacyTopconCl300Path(measurement.SourcePath));
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldRepairLegacyTopconCl300BuiltInExportProfile()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: new[] { CreateLegacyTopconCl300ExportProfile(isBuiltIn: true) },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.ExportProfiles, profile => profile.Metadata.Id == "export-medistar-topcon-cl300-default");
        Assert.True(profile.Metadata.IsBuiltIn);
        Assert.False(profile.Metadata.IsUserDefined);
        Assert.Contains(profile.Rules, rule => rule.SourcePath == "Device.Measure[@Type='LM']/LM/R/MedistarLine");
        Assert.Contains(profile.Rules, rule => rule.SourcePath == "Device.Measure[@Type='LM']/LM/L/MedistarLine");
        Assert.DoesNotContain(profile.Rules, rule => ContainsLegacyTopconCl300Path(rule.SourcePath));
        Assert.DoesNotContain(profile.Rules, rule => ContainsLegacyTopconCl300Path(rule.OutputTemplate));
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldRepairLegacyTopconKr800SBuiltInExportProfile()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: new[] { CreateLegacyTopconKr800SExportProfile(isBuiltIn: true) },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.ExportProfiles, profile => profile.Metadata.Id == "export-medistar-topcon-kr800-default");
        Assert.True(profile.Metadata.IsBuiltIn);
        Assert.False(profile.Metadata.IsUserDefined);
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='REF']/REF/L/MedistarLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6221" && rule.SourcePath == "Device.Measure[@Type='KM']/KM/MedistarLine1");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6221" && rule.SourcePath == "Device.Measure[@Type='KM']/KM/MedistarLine2");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6227" && rule.SourcePath == "Device.Measure[@Type='SBJ']/MedistarLine1");
        Assert.DoesNotContain(profile.Rules, rule => rule.TargetFieldCode == "6228" && ContainsLegacyTopconKr800SPath(rule.OutputTemplate));
        Assert.DoesNotContain(profile.Rules, rule => rule.TargetFieldCode == "6228" && ContainsLegacyTopconKr800SPath(rule.SourcePath));
        Assert.DoesNotContain(profile.Rules, rule =>
            rule.TargetFieldCode == "6228"
            && ((rule.SourcePath?.Contains("Measure[@Type='KM']", StringComparison.OrdinalIgnoreCase) ?? false)
                || (rule.OutputTemplate?.Contains("K1=", StringComparison.OrdinalIgnoreCase) ?? false)));
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldNotRepairUserDefinedTopconKr800SExportProfile()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: new[] { CreateLegacyTopconKr800SExportProfile(isBuiltIn: false) },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.ExportProfiles, profile => profile.Metadata.Id == "export-medistar-topcon-kr800-default");
        Assert.False(profile.Metadata.IsBuiltIn);
        Assert.True(profile.Metadata.IsUserDefined);
        Assert.Contains(profile.Rules, rule => ContainsLegacyTopconKr800SPath(rule.OutputTemplate));
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6228" && (rule.OutputTemplate?.Contains("K1=", StringComparison.OrdinalIgnoreCase) ?? false));
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldRepairLegacyTopconTrk2PBuiltInDeviceProfile()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: new[] { CreateLegacyTopconTrk2PDeviceProfile(isBuiltIn: true) },
            ExportProfiles: Array.Empty<ExportProfileDefinition>(),
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.DeviceProfiles, profile => profile.Metadata.Id == "device-topcon-trk2p-default");
        Assert.True(profile.Metadata.IsBuiltIn);
        Assert.False(profile.Metadata.IsUserDefined);
        Assert.Equal("TRK-2P", profile.Model);
        Assert.Contains(profile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains(profile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/HeaderLine");
        Assert.Contains(profile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/MeasuredRightLine");
        Assert.Contains(profile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/ParameterLeftLine");
        Assert.Contains(profile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/TonoListLine");
        Assert.Contains(profile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='CCT']/Pachy/HeaderLine");
        Assert.DoesNotContain(profile.Measurements, measurement => ContainsLegacyTopconTrk2PPath(measurement.SourcePath));
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldRepairLegacyTopconTrk2PBuiltInExportProfile()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: new[] { CreateLegacyTopconTrk2PExportProfile(isBuiltIn: true) },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.ExportProfiles, profile => profile.Metadata.Id == "export-medistar-topcon-trk2p-default");
        Assert.True(profile.Metadata.IsBuiltIn);
        Assert.False(profile.Metadata.IsUserDefined);
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6221" && rule.SourcePath == "Device.Measure[@Type='KM']/KM/MedistarLine1");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6220" && rule.SourcePath == "Device.Measure[@Type='CCT']/Pachy/HeaderLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6220" && rule.SourcePath == "Device.Measure[@Type='CCT']/Pachy/MedistarLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='TM']/Tono/HeaderLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='TM']/Tono/MeasuredRightLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='TM']/Tono/ParameterLeftLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='TM']/Tono/TonoListLine");
        Assert.DoesNotContain(profile.Rules, rule => rule.TargetFieldCode == "6228" && ContainsLegacyTopconTrk2PPath(rule.OutputTemplate));
        Assert.DoesNotContain(profile.Rules, rule => rule.TargetFieldCode == "6228" && ContainsLegacyTopconTrk2PPath(rule.SourcePath));
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldNotRepairUserDefinedTopconTrk2PExportProfile()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: new[] { CreateLegacyTopconTrk2PExportProfile(isBuiltIn: false) },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.ExportProfiles, profile => profile.Metadata.Id == "export-medistar-topcon-trk2p-default");
        Assert.False(profile.Metadata.IsBuiltIn);
        Assert.True(profile.Metadata.IsUserDefined);
        Assert.Contains(profile.Rules, rule => ContainsLegacyTopconTrk2PPath(rule.OutputTemplate));
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldRepairLegacyTopconCv5000BuiltInDeviceProfile()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: new[] { CreateLegacyTopconCv5000DeviceProfile(isBuiltIn: true) },
            ExportProfiles: Array.Empty<ExportProfileDefinition>(),
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.DeviceProfiles, profile => profile.Metadata.Id == "device-topcon-cv5000-default");
        Assert.True(profile.Metadata.IsBuiltIn);
        Assert.False(profile.Metadata.IsUserDefined);
        Assert.Contains(profile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='SBJ']/Prescription/HeaderLine");
        Assert.Contains(profile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='SBJ']/Prescription/R/MedistarLine");
        Assert.Contains(profile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='SBJ']/FullCorrection/R/MedistarLine");
        Assert.DoesNotContain(profile.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='SBJ']/MedistarLine", StringComparison.Ordinal));
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldRepairLegacyTopconCv5000BuiltInExportProfile()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: new[] { CreateLegacyTopconCv5000ExportProfile(isBuiltIn: true) },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.ExportProfiles, profile => profile.Metadata.Id == "export-medistar-topcon-cv5000-default");
        Assert.True(profile.Metadata.IsBuiltIn);
        Assert.False(profile.Metadata.IsUserDefined);
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='SBJ']/Prescription/HeaderLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='SBJ']/Prescription/R/MedistarLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6227" && rule.SourcePath == "Device.Measure[@Type='SBJ']/FullCorrection/R/MedistarLine");
        Assert.DoesNotContain(profile.Rules, rule => rule.TargetFieldCode == "6330");
        Assert.DoesNotContain(profile.Rules, rule => rule.SourcePath?.Contains("Device.Measure[@Type='SBJ']/MedistarLine", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(profile.Rules, rule => rule.TargetName == "PhoropterSeparator");
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldRepairTopconCv5000BuiltInExportProfileWithLegacy6330Rules()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: new[] { CreateTopconCv5000ExportProfileWithLegacy6330Rules(isBuiltIn: true) },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.ExportProfiles, profile => profile.Metadata.Id == "export-medistar-topcon-cv5000-default");
        Assert.True(profile.Metadata.IsBuiltIn);
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='SBJ']/Prescription/HeaderLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6227" && rule.SourcePath == "Device.Measure[@Type='SBJ']/FullCorrection/R/MedistarLine");
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6227" && rule.SourcePath == "Device.Measure[@Type='SBJ']/FullCorrection/L/MedistarLine");
        Assert.DoesNotContain(profile.Rules, rule => rule.TargetFieldCode == "6330");
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldNotRepairUserDefinedTopconCv5000ExportProfile()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: new[] { CreateLegacyTopconCv5000ExportProfile(isBuiltIn: false) },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.ExportProfiles, profile => profile.Metadata.Id == "export-medistar-topcon-cv5000-default");
        Assert.False(profile.Metadata.IsBuiltIn);
        Assert.True(profile.Metadata.IsUserDefined);
        Assert.Contains(profile.Rules, rule => rule.SourcePath == "Device.Measure[@Type='SBJ']/MedistarLine1");
        Assert.Contains(profile.Rules, rule => rule.TargetName == "PhoropterSeparator");
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldNotRepairUserDefinedTopconCv5000ExportProfileWithLegacy6330Rules()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: new[] { CreateTopconCv5000ExportProfileWithLegacy6330Rules(isBuiltIn: false) },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        var profile = Assert.Single(catalog.ExportProfiles, profile => profile.Metadata.Id == "export-medistar-topcon-cv5000-default");
        Assert.False(profile.Metadata.IsBuiltIn);
        Assert.True(profile.Metadata.IsUserDefined);
        Assert.Contains(profile.Rules, rule => rule.TargetFieldCode == "6330" && rule.SourcePath == "Device.Measure[@Type='SBJ']/FullCorrection/R/MedistarLine");
    }


    [Fact]
    public void Load_ShouldReadAllExpectedProfilesAfterEnsureDefaultProfiles()
    {
        var paths = CreateAppDataPaths();

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        Assert.Single(catalog.AisProfiles);
        Assert.Equal(18, catalog.DeviceProfiles.Count);
        Assert.Equal(18, catalog.ExportProfiles.Count);
        Assert.Equal(18, catalog.InterfaceProfiles.Count);
        AssertExpectedDeviceDefaults(catalog);
        AssertExpectedExportDefaults(catalog);
    }

    [Fact]
    public void Load_ShouldThrowInvalidOperationExceptionForInvalidJson()
    {
        var paths = CreateAppDataPaths();
        var aisFolder = Path.Combine(paths.ProfilesFolder, "ais");
        Directory.CreateDirectory(aisFolder);
        File.WriteAllText(Path.Combine(aisFolder, "invalid.json"), "{ invalid json");

        var exception = Assert.Throws<InvalidOperationException>(() => _service.Load(paths));

        Assert.Contains("Invalid profile JSON file", exception.Message);
    }

    [Fact]
    public void Save_ShouldWriteProfilesToExpectedSubFolders()
    {
        var paths = CreateAppDataPaths();

        _service.Save(paths, CreateDefaultCatalog());

        Assert.True(Directory.Exists(Path.Combine(paths.ProfilesFolder, "ais")));
        Assert.True(Directory.Exists(Path.Combine(paths.ProfilesFolder, "devices")));
        Assert.True(Directory.Exists(Path.Combine(paths.ProfilesFolder, "exports")));
        Assert.True(Directory.Exists(Path.Combine(paths.ProfilesFolder, "interfaces")));
    }

    [Fact]
    public void SaveNewExportProfile_ShouldWriteExportProfileWithoutCatalogSave()
    {
        var paths = CreateAppDataPaths();
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserExportMetadata("export-user-copy")
        };

        _service.SaveNewExportProfile(paths, profile);

        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "exports", "export-user-copy.json")));
        var catalog = _service.Load(paths);
        var loadedProfile = Assert.Single(catalog.ExportProfiles);
        Assert.Equal("export-user-copy", loadedProfile.Metadata.Id);
        Assert.True(loadedProfile.Metadata.IsUserDefined);
    }

    [Fact]
    public void SaveNewExportProfile_ShouldNotOverwriteExistingExportProfile()
    {
        var paths = CreateAppDataPaths();
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserExportMetadata("export-user-copy")
        };
        _service.SaveNewExportProfile(paths, profile);

        var exception = Assert.Throws<InvalidOperationException>(() => _service.SaveNewExportProfile(paths, profile));

        Assert.Contains("will not be overwritten", exception.Message);
    }

    [Fact]
    public void SaveInterfaceProfileDefinition_ShouldWriteUserDefinedProfile()
    {
        var paths = CreateAppDataPaths();
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserInterfaceMetadata("interface-user")
        };

        _service.SaveInterfaceProfileDefinition(paths, profile, overwriteExisting: false);

        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "interfaces", "interface-user.json")));
        var catalog = _service.Load(paths);
        var loadedProfile = Assert.Single(catalog.InterfaceProfiles);
        Assert.Equal("interface-user", loadedProfile.Metadata.Id);
        Assert.True(loadedProfile.Metadata.IsUserDefined);
    }

    [Fact]
    public void SaveInterfaceProfileDefinition_ShouldAllowOverwriteWhenRequested()
    {
        var paths = CreateAppDataPaths();
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserInterfaceMetadata("interface-user")
        };
        _service.SaveInterfaceProfileDefinition(paths, profile, overwriteExisting: false);

        var updatedProfile = profile with { IsActive = true };
        _service.SaveInterfaceProfileDefinition(paths, updatedProfile, overwriteExisting: true);

        var loadedProfile = Assert.Single(_service.Load(paths).InterfaceProfiles);
        Assert.True(loadedProfile.IsActive);
    }

    [Fact]
    public void SaveInterfaceProfileDefinition_ShouldReloadActiveLicenseRequiredUserDefinedProfile()
    {
        var paths = CreateAppDataPaths();
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserInterfaceMetadata("interface-active-license-required"),
            IsActive = true,
            IsLicenseRequired = true
        };

        _service.SaveInterfaceProfileDefinition(paths, profile, overwriteExisting: false);

        var loadedProfile = Assert.Single(_service.Load(paths).InterfaceProfiles);
        Assert.True(loadedProfile.IsActive);
        Assert.True(loadedProfile.IsLicenseRequired);
    }

    [Fact]
    public void SaveInterfaceProfileDefinition_ShouldPreserveAttachmentFolders()
    {
        var paths = CreateAppDataPaths();
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserInterfaceMetadata("interface-attachments"),
            FolderOptions = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions with
            {
                AttachmentImportFolder = @"C:\XdtDeviceBridge\GAImport",
                AttachmentExportFolder = @"C:\XdtDeviceBridge\GAExport",
                AttachmentFileNameTemplate = "GA_{Ais.PatientNumber}{ExtensionUpper}",
                AttachmentTransferMode = AttachmentTransferMode.Move,
                AttachmentExternalLinkDocumentName = "PDF-Befund",
                AttachmentExternalLinkFileFormat = "{ExtensionUpperWithoutDot}",
                AttachmentExternalLinkDescription = "Messprotokoll Autorefraktor",
                AttachmentExternalLinkPathTemplate = "{Attachment.TargetFullPath}",
                IsAttachmentProcessingEnabled = true
            }
        };

        _service.SaveInterfaceProfileDefinition(paths, profile, overwriteExisting: false);

        var loadedProfile = Assert.Single(_service.Load(paths).InterfaceProfiles);
        Assert.Equal(@"C:\XdtDeviceBridge\GAImport", loadedProfile.FolderOptions.AttachmentImportFolder);
        Assert.Equal(@"C:\XdtDeviceBridge\GAExport", loadedProfile.FolderOptions.AttachmentExportFolder);
        Assert.Equal("GA_{Ais.PatientNumber}{ExtensionUpper}", loadedProfile.FolderOptions.AttachmentFileNameTemplate);
        Assert.Equal(AttachmentTransferMode.Move, loadedProfile.FolderOptions.AttachmentTransferMode);
        Assert.Equal("PDF-Befund", loadedProfile.FolderOptions.AttachmentExternalLinkDocumentName);
        Assert.Equal("{ExtensionUpperWithoutDot}", loadedProfile.FolderOptions.AttachmentExternalLinkFileFormat);
        Assert.Equal("Messprotokoll Autorefraktor", loadedProfile.FolderOptions.AttachmentExternalLinkDescription);
        Assert.Equal("{Attachment.TargetFullPath}", loadedProfile.FolderOptions.AttachmentExternalLinkPathTemplate);
        Assert.True(loadedProfile.FolderOptions.IsAttachmentProcessingEnabled);
    }

    [Fact]
    public void DeleteInterfaceProfile_ShouldDeleteUserDefinedInterfaceProfile()
    {
        var paths = CreateAppDataPaths();
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserInterfaceMetadata("interface-user")
        };
        _service.SaveInterfaceProfileDefinition(paths, profile, overwriteExisting: false);

        var deleted = _service.DeleteInterfaceProfile(paths, "interface-user");

        Assert.True(deleted);
        Assert.Empty(_service.Load(paths).InterfaceProfiles);
        Assert.False(File.Exists(Path.Combine(paths.ProfilesFolder, "interfaces", "interface-user.json")));
    }

    [Fact]
    public void DeleteInterfaceProfile_ShouldNotDeleteAisDeviceOrExportProfiles()
    {
        var paths = CreateAppDataPaths();
        var userInterfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserInterfaceMetadata("interface-user")
        };
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: new[] { DefaultAisProfiles.CreateMedistarDefault() },
            DeviceProfiles: new[] { DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() },
            ExportProfiles: new[] { DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() },
            InterfaceProfiles: new[] { userInterfaceProfile }));

        _service.DeleteInterfaceProfile(paths, "interface-user");

        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "ais", "ais-medistar-default.json")));
        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "devices", "device-nidek-ark1s-default.json")));
        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "exports", "export-medistar-nidek-ark1s-default.json")));
        var catalog = _service.Load(paths);
        Assert.Single(catalog.AisProfiles);
        Assert.Single(catalog.DeviceProfiles);
        Assert.Single(catalog.ExportProfiles);
        Assert.Empty(catalog.InterfaceProfiles);
    }

    [Fact]
    public void DeleteInterfaceProfile_ShouldReturnFalseForUnknownId()
    {
        var paths = CreateAppDataPaths();

        var deleted = _service.DeleteInterfaceProfile(paths, "interface-missing");

        Assert.False(deleted);
    }

    [Fact]
    public void DeleteInterfaceProfile_ShouldRejectBuiltInProfile()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, CreateDefaultCatalog());

        var exception = Assert.Throws<InvalidOperationException>(() =>
            _service.DeleteInterfaceProfile(paths, "interface-medistar-nidek-ark1s-default"));

        Assert.Contains("Built-in interface profiles cannot be deleted.", exception.Message);
        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "interfaces", "interface-medistar-nidek-ark1s-default.json")));
    }

    private static AppDataPaths CreateAppDataPaths()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return new AppDataPathProvider().GetPaths(baseFolder);
    }

    private static ProfileCatalog CreateDefaultCatalog()
    {
        return new ProfileCatalog(
            AisProfiles: new[] { DefaultAisProfiles.CreateMedistarDefault() },
            DeviceProfiles: new[] { DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() },
            ExportProfiles: new[] { DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() },
            InterfaceProfiles: new[] { DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() });
    }

    private static void AssertExpectedDeviceDefaults(ProfileCatalog catalog)
    {
        var ids = catalog.DeviceProfiles.Select(profile => profile.Metadata.Id).ToHashSet(StringComparer.Ordinal);

        Assert.Contains("device-nidek-ark1s-default", ids);
        Assert.Contains("device-nidek-ar360-default", ids);
        Assert.Contains("device-nidek-lm7-default", ids);
        Assert.Contains("device-nidek-nt530p-default", ids);
        Assert.Contains("device-nidek-rt6100-default", ids);
        Assert.Contains("device-nidek-rt2100-serial-default", ids);
        Assert.Contains("device-nidek-rt3100-serial-default", ids);
        Assert.Contains("device-nidek-rt5100-serial-default", ids);
        Assert.Contains("device-topcon-cl300-default", ids);
        Assert.Contains("device-topcon-solos-default", ids);
        Assert.Contains("device-topcon-kr800-default", ids);
        Assert.Contains("device-topcon-kr1-default", ids);
        Assert.Contains("device-topcon-trk2p-default", ids);
        Assert.Contains("device-topcon-ct1p-default", ids);
        Assert.Contains("device-topcon-ct800a-default", ids);
        Assert.Contains("device-topcon-cv5000-default", ids);
        Assert.Contains("device-document-attachment-default", ids);
        Assert.Contains("device-manual-document-selection-default", ids);
    }

    private static void AssertExpectedExportDefaults(ProfileCatalog catalog)
    {
        var ids = catalog.ExportProfiles.Select(profile => profile.Metadata.Id).ToHashSet(StringComparer.Ordinal);

        Assert.Contains("export-medistar-nidek-ark1s-default", ids);
        Assert.Contains("export-medistar-nidek-ar360-default", ids);
        Assert.Contains("export-medistar-nidek-lm7-default", ids);
        Assert.Contains("export-medistar-nidek-nt530p-default", ids);
        Assert.Contains("export-medistar-nidek-rt6100-default", ids);
        Assert.Contains("export-medistar-nidek-rt2100-serial-default", ids);
        Assert.Contains("export-medistar-nidek-rt3100-serial-default", ids);
        Assert.Contains("export-medistar-nidek-rt5100-serial-default", ids);
        Assert.Contains("export-medistar-topcon-cl300-default", ids);
        Assert.Contains("export-medistar-topcon-solos-default", ids);
        Assert.Contains("export-medistar-topcon-kr800-default", ids);
        Assert.Contains("export-medistar-topcon-kr1-default", ids);
        Assert.Contains("export-medistar-topcon-trk2p-default", ids);
        Assert.Contains("export-medistar-topcon-ct1p-default", ids);
        Assert.Contains("export-medistar-topcon-ct800a-default", ids);
        Assert.Contains("export-medistar-topcon-cv5000-default", ids);
        Assert.Contains("export-medistar-document-attachment-default", ids);
        Assert.Contains("export-medistar-manual-document-transfer-default", ids);
    }

    private static ExportProfileDefinition CreateLegacyLm7ExportProfile(bool isBuiltIn)
    {
        var current = DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default();
        return current with
        {
            Metadata = current.Metadata with
            {
                IsBuiltIn = isBuiltIn,
                IsUserDefined = !isBuiltIn
            },
            Rules = current.Rules.Take(6)
                .Concat(new[]
                {
                    new ExportRuleDefinition(
                        "7",
                        "6228",
                        "LensmeterResultRight",
                        ExportRuleType.Template,
                        null,
                        "R.:S={Device.R/LM/Median/Sphere:Diopter} Z={Device.R/LM/Median/Cylinder:Diopter}*{Device.R/LM/Median/Axis:Axis} P={Device.R/LM/Median/PrismHorizontal:Prism} {Device.R/LM/Median/PrismHorizontalBase:Raw} {Device.R/LM/Median/PrismVertical:Prism} {Device.R/LM/Median/PrismVerticalBase:Raw}              PD={Device.PD/Distance:Pd}",
                        7,
                        true,
                        "Legacy LM7 right lensmeter template with unresolved median paths."),
                    new ExportRuleDefinition(
                        "8",
                        "6228",
                        "LensmeterResultLeft",
                        ExportRuleType.Template,
                        null,
                        "L.:S={Device.L/LM/Median/Sphere:Diopter} Z={Device.L/LM/Median/Cylinder:Diopter}*{Device.L/LM/Median/Axis:Axis} P={Device.L/LM/Median/PrismHorizontal:Prism} {Device.L/LM/Median/PrismHorizontalBase:Raw} {Device.L/LM/Median/PrismVertical:Prism} {Device.L/LM/Median/PrismVerticalBase:Raw}",
                        8,
                        true,
                        "Legacy LM7 left lensmeter template with unresolved median paths.")
                })
                .ToArray()
        };
    }

    private static DeviceProfileDefinition CreateLegacyTopconCl300DeviceProfile(bool isBuiltIn)
    {
        var current = DefaultDeviceProfileDefinitions.CreateTopconCl300Default();
        return current with
        {
            Metadata = current.Metadata with
            {
                IsBuiltIn = isBuiltIn,
                IsUserDefined = !isBuiltIn
            },
            Measurements = current.Measurements
                .Where(measurement => !measurement.SourcePath.EndsWith("/MedistarLine", StringComparison.Ordinal))
                .Select(measurement => measurement with
                {
                    SourcePath = measurement.SourcePath.Replace("Measure[@Type='LM']/", "Ophthalmology/Measure[@type='LM']/", StringComparison.Ordinal)
                })
                .ToArray()
        };
    }

    private static ExportProfileDefinition CreateLegacyTopconCl300ExportProfile(bool isBuiltIn)
    {
        var current = DefaultExportProfileDefinitions.CreateMedistarTopconCl300Default();
        return current with
        {
            Metadata = current.Metadata with
            {
                IsBuiltIn = isBuiltIn,
                IsUserDefined = !isBuiltIn
            },
            Rules = current.Rules.Take(6)
                .Concat(new[]
                {
                    new ExportRuleDefinition(
                        "7",
                        "6228",
                        "LensmeterResultRight",
                        ExportRuleType.Template,
                        "Device.Ophthalmology/Measure[@type='LM']/LM/R/Sphere",
                        "R.:S={Device.Ophthalmology/Measure[@type='LM']/LM/R/Sphere} Z={Device.Ophthalmology/Measure[@type='LM']/LM/R/Cylinder}*{Device.Ophthalmology/Measure[@type='LM']/LM/R/Axis}",
                        7,
                        true,
                        "Legacy TOPCON CL300 right lensmeter template with root-prefixed paths."),
                    new ExportRuleDefinition(
                        "8",
                        "6228",
                        "LensmeterResultLeft",
                        ExportRuleType.Template,
                        "Device.Ophthalmology/Measure[@type='LM']/LM/L/Sphere",
                        "L.:S={Device.Ophthalmology/Measure[@type='LM']/LM/L/Sphere} Z={Device.Ophthalmology/Measure[@type='LM']/LM/L/Cylinder}*{Device.Ophthalmology/Measure[@type='LM']/LM/L/Axis}",
                        8,
                        true,
                        "Legacy TOPCON CL300 left lensmeter template with root-prefixed paths.")
                })
                .ToArray()
        };
    }

    private static ExportProfileDefinition CreateLegacyTopconKr800SExportProfile(bool isBuiltIn)
    {
        var current = DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default();
        return current with
        {
            Metadata = current.Metadata with
            {
                IsBuiltIn = isBuiltIn,
                IsUserDefined = !isBuiltIn
            },
            Rules = current.Rules.Take(6)
                .Concat(new[]
                {
                    new ExportRuleDefinition(
                        "7",
                        "6228",
                        "RefResultRight",
                        ExportRuleType.Template,
                        null,
                        "R.:S={Device.Ophthalmology/Measure[@type='REF']/REF/R/Sphere} Z={Device.Ophthalmology/Measure[@type='REF']/REF/R/Cylinder}*{Device.Ophthalmology/Measure[@type='REF']/REF/R/Axis}                              PD={Device.Ophthalmology/Measure[@type='REF']/PD/Distance}",
                        7,
                        true,
                        "Legacy TOPCON KR800S right REF template with root-prefixed paths."),
                    new ExportRuleDefinition(
                        "8",
                        "6228",
                        "RefResultLeft",
                        ExportRuleType.Template,
                        null,
                        "L.:S={Device.Ophthalmology/Measure[@type='REF']/REF/L/Sphere} Z={Device.Ophthalmology/Measure[@type='REF']/REF/L/Cylinder}*{Device.Ophthalmology/Measure[@type='REF']/REF/L/Axis}                              PD=",
                        8,
                        true,
                        "Legacy TOPCON KR800S left REF template with root-prefixed paths."),
                    new ExportRuleDefinition(
                        "9",
                        "6228",
                        "KeratometryRight",
                        ExportRuleType.Template,
                        null,
                        "KR: K1={Device.Ophthalmology/Measure[@type='KM']/KM/R/K1/Power}*{Device.Ophthalmology/Measure[@type='KM']/KM/R/K1/Axis} K2={Device.Ophthalmology/Measure[@type='KM']/KM/R/K2/Power}*{Device.Ophthalmology/Measure[@type='KM']/KM/R/K2/Axis}",
                        9,
                        true,
                        "Legacy TOPCON KR800S KM template wrongly emitted through 6228."),
                    new ExportRuleDefinition(
                        "10",
                        "6228",
                        "KeratometryLeft",
                        ExportRuleType.Template,
                        null,
                        "KL: K1={Device.Ophthalmology/Measure[@type='KM']/KM/L/K1/Power}*{Device.Ophthalmology/Measure[@type='KM']/KM/L/K1/Axis} K2={Device.Ophthalmology/Measure[@type='KM']/KM/L/K2/Power}*{Device.Ophthalmology/Measure[@type='KM']/KM/L/K2/Axis}",
                        10,
                        true,
                        "Legacy TOPCON KR800S KM template wrongly emitted through 6228.")
                })
                .ToArray()
        };
    }

    private static DeviceProfileDefinition CreateLegacyTopconTrk2PDeviceProfile(bool isBuiltIn)
    {
        var current = DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault();
        return current with
        {
            Metadata = current.Metadata with
            {
                IsBuiltIn = isBuiltIn,
                IsUserDefined = !isBuiltIn
            },
            Model = "TRK2P",
            DeviceType = "Tonometer/Pachymeter",
            Measurements = current.Measurements
                .Where(measurement => !measurement.SourcePath.Contains("MedistarLine", StringComparison.Ordinal))
                .Select(measurement => measurement with
                {
                    SourcePath = measurement.SourcePath.StartsWith("Measure[", StringComparison.Ordinal)
                        ? $"Ophthalmology/{measurement.SourcePath.Replace("[@Type=", "[@type=", StringComparison.Ordinal)}"
                        : measurement.SourcePath
                })
                .ToArray()
        };
    }

    private static ExportProfileDefinition CreateLegacyTopconTrk2PExportProfile(bool isBuiltIn)
    {
        var current = DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault();
        return current with
        {
            Metadata = current.Metadata with
            {
                IsBuiltIn = isBuiltIn,
                IsUserDefined = !isBuiltIn
            },
            Rules = current.Rules.Take(6)
                .Concat(new[]
                {
                    new ExportRuleDefinition(
                        "7",
                        "6228",
                        "TonometryBothEyes",
                        ExportRuleType.Template,
                        null,
                        "R = {Device.Ophthalmology/Measure[@type='TM']/TM/R/Average/IOP_mmHg:Iop} // L = {Device.Ophthalmology/Measure[@type='TM']/TM/L/Average/IOP_mmHg:Iop} mmHg",
                        7,
                        true,
                        "Legacy TOPCON TRK2P tonometry template wrongly emitted through 6228."),
                    new ExportRuleDefinition(
                        "8",
                        "6228",
                        "PachymetryRight",
                        ExportRuleType.Template,
                        null,
                        "PR: {Device.Ophthalmology/Measure[@type='TM']/CCT/R/List[@No='3']/CCT_mm:Pachy} µm",
                        8,
                        true,
                        "Legacy TOPCON TRK2P pachymetry template wrongly emitted through 6228.")
                })
                .ToArray()
        };
    }

    private static DeviceProfileDefinition CreateLegacyTopconCv5000DeviceProfile(bool isBuiltIn)
    {
        var current = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default();
        return current with
        {
            Metadata = current.Metadata with
            {
                IsBuiltIn = isBuiltIn,
                IsUserDefined = !isBuiltIn
            },
            Measurements = current.Measurements
                .Where(measurement =>
                    !measurement.SourcePath.Contains("/Prescription/", StringComparison.Ordinal)
                    && !measurement.SourcePath.Contains("/FullCorrection/", StringComparison.Ordinal))
                .Concat(new[]
                {
                    new DeviceMeasurementDefinition("cv5000-sbj-line1", "SBJ MEDISTAR-Zeile 1", "Measure[@Type='SBJ']/MedistarLine1", "SBJ", string.Empty, string.Empty, false, "Legacy computed MEDISTAR 6228 line 1."),
                    new DeviceMeasurementDefinition("cv5000-sbj-line2", "SBJ MEDISTAR-Zeile 2", "Measure[@Type='SBJ']/MedistarLine2", "SBJ", string.Empty, string.Empty, false, "Legacy computed MEDISTAR 6228 line 2."),
                    new DeviceMeasurementDefinition("cv5000-sbj-line3", "SBJ MEDISTAR-Zeile 3", "Measure[@Type='SBJ']/MedistarLine3", "SBJ", string.Empty, string.Empty, false, "Legacy computed MEDISTAR separator."),
                    new DeviceMeasurementDefinition("cv5000-sbj-line4", "SBJ MEDISTAR-Zeile 4", "Measure[@Type='SBJ']/MedistarLine4", "SBJ", string.Empty, string.Empty, false, "Legacy computed MEDISTAR 6228 line 4."),
                    new DeviceMeasurementDefinition("cv5000-sbj-line5", "SBJ MEDISTAR-Zeile 5", "Measure[@Type='SBJ']/MedistarLine5", "SBJ", string.Empty, string.Empty, false, "Legacy computed MEDISTAR 6228 line 5.")
                })
                .ToArray()
        };
    }

    private static ExportProfileDefinition CreateLegacyTopconCv5000ExportProfile(bool isBuiltIn)
    {
        var current = DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default();
        return current with
        {
            Metadata = current.Metadata with
            {
                IsBuiltIn = isBuiltIn,
                IsUserDefined = !isBuiltIn
            },
            Rules = current.Rules.Take(6)
                .Concat(new[]
                {
                    new ExportRuleDefinition("7", "6228", "PhoropterLine1", ExportRuleType.Template, "Device.Measure[@Type='SBJ']/MedistarLine1", "{value}", 7, true, "Legacy CV-5000 phoropter return line 1."),
                    new ExportRuleDefinition("8", "6228", "PhoropterLine2", ExportRuleType.Template, "Device.Measure[@Type='SBJ']/MedistarLine2", "{value}", 8, true, "Legacy CV-5000 phoropter return line 2."),
                    new ExportRuleDefinition("9", "6228", "PhoropterSeparator", ExportRuleType.Template, "Device.Measure[@Type='SBJ']/MedistarLine3", "{value}", 9, true, "Legacy CV-5000 separator line."),
                    new ExportRuleDefinition("10", "6228", "PhoropterLine4", ExportRuleType.Template, "Device.Measure[@Type='SBJ']/MedistarLine4", "{value}", 10, true, "Legacy CV-5000 phoropter return line 4."),
                    new ExportRuleDefinition("11", "6228", "PhoropterLine5", ExportRuleType.Template, "Device.Measure[@Type='SBJ']/MedistarLine5", "{value}", 11, true, "Legacy CV-5000 phoropter return line 5."),
                    new ExportRuleDefinition("12", "6228", "PhoropterLine6", ExportRuleType.Template, "Device.Measure[@Type='SBJ']/MedistarLine6", "{value}", 12, true, "Legacy CV-5000 reserved line.")
                })
                .ToArray()
        };
    }

    private static ExportProfileDefinition CreateTopconCv5000ExportProfileWithLegacy6330Rules(bool isBuiltIn)
    {
        var current = DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default();
        return current with
        {
            Metadata = current.Metadata with
            {
                IsBuiltIn = isBuiltIn,
                IsUserDefined = !isBuiltIn
            },
            Rules = current.Rules
                .Select(rule =>
                {
                    if (string.Equals(rule.SourcePath, "Device.Measure[@Type='SBJ']/Prescription/HeaderLine", StringComparison.Ordinal))
                    {
                        return rule with { TargetFieldCode = "6227" };
                    }

                    if (rule.SourcePath?.Contains("Device.Measure[@Type='SBJ']/FullCorrection/", StringComparison.Ordinal) == true
                        && rule.SourcePath.EndsWith("/MedistarLine", StringComparison.Ordinal))
                    {
                        return rule with { TargetFieldCode = "6330" };
                    }

                    return rule;
                })
                .ToArray()
        };
    }

    private static bool ContainsLegacyLm7MedianPath(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && (value.Contains("Device.R/LM/Median/", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Device.L/LM/Median/", StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsLegacyTopconCl300Path(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains("Ophthalmology/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsLegacyTopconKr800SPath(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && (value.Contains("Device.Ophthalmology/", StringComparison.OrdinalIgnoreCase)
                || value.Contains("KR: K1=", StringComparison.OrdinalIgnoreCase)
                || value.Contains("KL: K1=", StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsLegacyTopconTrk2PPath(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && (value.Contains("Ophthalmology/", StringComparison.OrdinalIgnoreCase)
                || value.Contains(":Iop", StringComparison.OrdinalIgnoreCase)
                || value.Contains(":Pachy", StringComparison.OrdinalIgnoreCase));
    }

    private static ProfileMetadata CreateUserExportMetadata(string id)
    {
        var timestamp = new DateTimeOffset(2026, 5, 5, 12, 0, 0, TimeSpan.Zero);
        return new ProfileMetadata(
            Id: id,
            Name: "User Export",
            ProfileKind: ProfileKind.ExportProfile,
            Description: null,
            Vendor: null,
            Product: null,
            Version: "1.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "TestUser",
            IsBuiltIn: false,
            IsUserDefined: true);
    }

    private static ProfileMetadata CreateUserInterfaceMetadata(string id)
    {
        var timestamp = new DateTimeOffset(2026, 5, 5, 12, 0, 0, TimeSpan.Zero);
        return new ProfileMetadata(
            Id: id,
            Name: "User Interface",
            ProfileKind: ProfileKind.InterfaceProfile,
            Description: null,
            Vendor: null,
            Product: null,
            Version: "1.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "TestUser",
            IsBuiltIn: false,
            IsUserDefined: true);
    }
}
