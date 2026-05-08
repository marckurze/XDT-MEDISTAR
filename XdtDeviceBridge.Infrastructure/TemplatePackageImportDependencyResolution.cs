namespace XdtDeviceBridge.Infrastructure;

public enum TemplatePackageImportDependencyResolution
{
    LocalExisting,
    ImportedAsNew,
    ImportedAsCopy,
    Missing,
    Blocked
}
