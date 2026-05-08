namespace XdtDeviceBridge.Infrastructure;

public enum TemplatePackageImportConflictType
{
    None,
    SameIdExists,
    SameNameExists,
    BuiltInProtected,
    VersionMismatch,
    MissingDependency,
    InvalidProfile,
    UnsafeFolderPath,
    UnsupportedProfileKind
}
