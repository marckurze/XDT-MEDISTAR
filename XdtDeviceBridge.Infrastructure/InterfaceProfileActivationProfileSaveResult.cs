namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationProfileSaveResult(
    InterfaceProfileActivationProfileStoreStatus Status,
    bool Success,
    string ProfileId,
    string ProfileName,
    bool WouldSave,
    bool WasSaved,
    bool ProfileChanged,
    bool IsUserDefined,
    bool IsBuiltIn,
    string Message,
    IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> Preconditions);
