using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportResult(
    TemplatePackage Package,
    IReadOnlyList<AisProfile> AisProfiles,
    IReadOnlyList<DeviceProfileDefinition> DeviceProfiles,
    IReadOnlyList<ExportProfileDefinition> ExportProfiles,
    IReadOnlyList<InterfaceProfileDefinition> InterfaceProfiles);
