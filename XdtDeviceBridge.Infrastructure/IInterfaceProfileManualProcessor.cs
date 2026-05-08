using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public interface IInterfaceProfileManualProcessor
{
    InterfaceProfileManualProcessingResult Process(
        InterfaceProfileDefinition interfaceProfile,
        ExportProfileDefinition exportProfile,
        string aisFilePath,
        string deviceFilePath,
        DateTime timestamp,
        Func<PatientData, AttachmentProcessingStatus?>? attachmentPreparation = null);
}
