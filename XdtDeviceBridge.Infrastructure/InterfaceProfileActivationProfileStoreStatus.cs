namespace XdtDeviceBridge.Infrastructure;

public enum InterfaceProfileActivationProfileStoreStatus
{
    NotAvailable,
    NotFound,
    BuiltInBlocked,
    UserDefinedRequired,
    LoadedUserDefined,
    SaveBlocked,
    SaveNotImplemented,
    WouldSave,
    Saved
}
