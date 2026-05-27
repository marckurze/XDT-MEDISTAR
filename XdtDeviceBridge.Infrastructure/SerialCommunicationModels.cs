using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public interface ISerialPortDiscoveryService
{
    IReadOnlyList<string> GetAvailablePortNames();
}

public interface ISerialDeviceCommunicationService
{
    Task<SerialCommunicationSessionResult> ListenAsync(
        SerialCommunicationSettings settings,
        TimeSpan duration,
        CancellationToken cancellationToken);

    Task<SerialCommunicationWriteResult> WriteAsync(
        SerialCommunicationSettings settings,
        string command,
        CancellationToken cancellationToken);
}

public sealed record SerialCommunicationSessionResult(
    bool Success,
    string? ErrorMessage,
    string PortName,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    int BytesReceived,
    string RawText,
    string HexDump,
    IReadOnlyList<string> Lines);

public sealed record SerialCommunicationWriteResult(
    bool Success,
    string? ErrorMessage,
    string PortName,
    int BytesWritten);

public sealed record SerialRawDeviceInput(
    string PortName,
    SerialCommunicationSettings Settings,
    DateTimeOffset ReceivedAt,
    byte[] Bytes,
    string RawText,
    string HexDump);
