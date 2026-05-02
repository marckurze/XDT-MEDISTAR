namespace XdtDeviceBridge.Core;

public enum DeviceParserMode
{
    Xml,
    Unknown
}

public sealed record DeviceProfile(
    string Id,
    string Name,
    string AisImportFolder,
    string DeviceImportFolder,
    string ExportFolder,
    string ArchiveFolder,
    string ErrorFolder,
    string ExportFileNamePattern,
    DeviceParserMode DeviceParserMode,
    string OutputEncoding,
    bool AutoExport,
    int AssignmentWindowMinutes,
    List<MappingRule> MappingRules);
