using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationProfileLoadResult(
    InterfaceProfileActivationProfileStoreStatus Status,
    bool Success,
    InterfaceProfileDefinition? Profile,
    string ProfileId,
    string ProfileName,
    bool Found,
    bool IsUserDefined,
    bool IsBuiltIn,
    string Message,
    IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> Preconditions);
