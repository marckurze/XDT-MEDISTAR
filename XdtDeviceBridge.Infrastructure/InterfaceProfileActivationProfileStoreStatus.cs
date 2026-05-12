namespace XdtDeviceBridge.Infrastructure;

public enum InterfaceProfileActivationProfileStoreStatus
{
    NotAvailable,
    NotFound,
    BuiltInBlocked,
    NonUserDefinedBlocked,
    UserDefinedRequired,
    LoadedUserDefined,
    SaveNotImplemented,
    SaveWouldBeAllowed,
    MissingCapability,
    Failed
}
