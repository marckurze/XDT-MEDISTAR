using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record BuilderManualProcessingPreviewRequest(
    InterfaceProfileDefinition? InterfaceProfile,
    DeviceProfileDefinition? DeviceProfile,
    ExportProfileDefinition ExportProfile,
    string AisFilePath,
    string DeviceFilePath);
