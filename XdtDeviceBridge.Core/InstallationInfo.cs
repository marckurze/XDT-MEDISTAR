namespace XdtDeviceBridge.Core;

public sealed record InstallationInfo(
    string InstallationId,
    string MachineName,
    string UserName,
    bool IsTerminalServer,
    DateTime CreatedAt);
