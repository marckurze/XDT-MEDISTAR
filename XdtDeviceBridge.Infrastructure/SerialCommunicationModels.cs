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

    Task<SerialCommunicationExchangeResult> ExchangeAsync(
        SerialCommunicationSettings settings,
        SerialCommunicationExchangeRequest request,
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

public sealed record SerialCommunicationExchangeRequest(
    byte[] RequestBytes,
    byte[] ExpectedHandshakeBytes,
    byte[] PayloadBytes,
    byte EndOfTransmissionByte,
    TimeSpan HandshakeTimeout,
    TimeSpan ReceiveTimeout,
    TimeSpan StableAfterEndOfTransmission,
    int MaxReceiveBytes,
    bool ContinueWithoutHandshake = false,
    bool ReceiveResponse = true,
    TimeSpan SendDelayAfterRequest = default,
    bool ToggleDtrBeforeRequest = false,
    TimeSpan DtrResetDuration = default,
    TimeSpan DelayAfterDtrEnable = default,
    bool AppendCarriageReturnToRequest = false,
    bool AppendCarriageReturnToPayload = false);

public sealed record SerialCommunicationExchangeResult(
    bool Success,
    string? ErrorMessage,
    string PortName,
    int BytesWritten,
    byte[] HandshakeBytes,
    byte[] ReceivedBytes,
    string RawText,
    string HexDump,
    IReadOnlyList<string> Messages,
    SerialModemStatus? LastModemStatus = null);

public sealed record SerialModemStatus(
    bool? Cts,
    bool? Dsr,
    bool? CarrierDetect,
    bool? RingIndicator)
{
    public static SerialModemStatus Unavailable { get; } = new(null, null, null, null);
}

public sealed record SerialRawDeviceInput(
    string PortName,
    SerialCommunicationSettings Settings,
    DateTimeOffset ReceivedAt,
    byte[] Bytes,
    string RawText,
    string HexDump);
