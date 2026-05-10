namespace XdtDeviceBridge.Core;

public sealed record ExpectedInputDisplayItem(
    string Name,
    string FolderPath,
    string Status,
    string StatusClass,
    string Detail);
