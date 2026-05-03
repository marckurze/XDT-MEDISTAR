using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record ProfileCatalog(
    IReadOnlyList<AisProfile> AisProfiles,
    IReadOnlyList<DeviceProfileDefinition> DeviceProfiles,
    IReadOnlyList<ExportProfileDefinition> ExportProfiles,
    IReadOnlyList<InterfaceProfileDefinition> InterfaceProfiles);
