namespace XdtDeviceBridge.Infrastructure;

public enum InterfaceProfileActivationProfileStoreStatus
{
    NotAvailable,
    NotFound,
    BuiltInBlocked,
    NonUserDefinedBlocked,
    UserDefinedRequired,
    LoadedUserDefined,
    ValidateOnly,
    SaveBlocked,
    SaveNotImplemented,
    SaveWouldBeAllowed,
    WouldSave,
    Saved,
    MissingCapability,
    Failed
}
