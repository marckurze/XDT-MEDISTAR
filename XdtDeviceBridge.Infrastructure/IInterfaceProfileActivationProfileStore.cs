namespace XdtDeviceBridge.Infrastructure;

public interface IInterfaceProfileActivationProfileStore
{
    InterfaceProfileActivationProfileLoadResult LoadFreshUserDefinedProfile(string profileId);

    InterfaceProfileActivationProfileSaveResult SaveUserDefinedProfile(
        InterfaceProfileActivationProfileSaveRequest request);
}
