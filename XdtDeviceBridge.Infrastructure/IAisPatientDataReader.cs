namespace XdtDeviceBridge.Infrastructure;

public interface IAisPatientDataReader
{
    AisPatientDataReadResult Read(string aisFilePath);
}
