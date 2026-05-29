using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public interface INidekRtSerialPhoropterCommunicationService
{
    Task<NidekRtSerialPhoropterCommunicationResult> SendSelectionAndReceiveAsync(
        SerialCommunicationSettings settings,
        PatientData patient,
        IReadOnlyList<AisHistoricalMeasurementRecord> selectedMeasurements,
        NidekRtSerialPhoropterModel model,
        CancellationToken cancellationToken);

    Task<NidekRtSerialPhoropterCommunicationResult> ReceiveReturnAsync(
        SerialCommunicationSettings settings,
        CancellationToken cancellationToken);
}

public sealed record NidekRtSerialPhoropterCommunicationOptions(
    TimeSpan HandshakeTimeout,
    TimeSpan ReceiveTimeout,
    TimeSpan StableAfterEndOfTransmission,
    int MaxReceiveBytes)
{
    public static NidekRtSerialPhoropterCommunicationOptions Default { get; } = new(
        HandshakeTimeout: TimeSpan.FromSeconds(8),
        ReceiveTimeout: TimeSpan.FromSeconds(60),
        StableAfterEndOfTransmission: TimeSpan.FromMilliseconds(800),
        MaxReceiveBytes: 128 * 1024);
}

public sealed record NidekRtSerialPhoropterCommunicationResult(
    bool Success,
    string? ErrorMessage,
    string PortName,
    byte[] RequestBytes,
    byte[] SentBytes,
    byte[] HandshakeBytes,
    byte[] ReceivedBytes,
    string ReceivedText,
    string ReceivedHexDump,
    IReadOnlyList<string> Messages);

public sealed class NidekRtSerialPhoropterCommunicationService : INidekRtSerialPhoropterCommunicationService
{
    private static readonly byte[] RsRequestBytes =
    {
        NidekRtSerialControlChars.SH,
        (byte)'C',
        (byte)' ',
        (byte)'*',
        (byte)'*',
        NidekRtSerialControlChars.SX,
        (byte)'R',
        (byte)'S',
        NidekRtSerialControlChars.EB,
        NidekRtSerialControlChars.ET
    };

    private static readonly byte[] ReadyToSendMarker =
    {
        NidekRtSerialControlChars.SX,
        (byte)'S',
        (byte)'D'
    };

    private readonly ISerialDeviceCommunicationService _serialCommunicationService;
    private readonly NidekRtSerialPhoropterOutputWriter _writer;
    private readonly NidekRtSerialPhoropterCommunicationOptions _options;

    public NidekRtSerialPhoropterCommunicationService(ISerialDeviceCommunicationService serialCommunicationService)
        : this(serialCommunicationService, new NidekRtSerialPhoropterOutputWriter(), NidekRtSerialPhoropterCommunicationOptions.Default)
    {
    }

    public NidekRtSerialPhoropterCommunicationService(
        ISerialDeviceCommunicationService serialCommunicationService,
        NidekRtSerialPhoropterOutputWriter writer,
        NidekRtSerialPhoropterCommunicationOptions options)
    {
        _serialCommunicationService = serialCommunicationService ?? throw new ArgumentNullException(nameof(serialCommunicationService));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Task<NidekRtSerialPhoropterCommunicationResult> SendSelectionAndReceiveAsync(
        SerialCommunicationSettings settings,
        PatientData patient,
        IReadOnlyList<AisHistoricalMeasurementRecord> selectedMeasurements,
        NidekRtSerialPhoropterModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(patient);
        ArgumentNullException.ThrowIfNull(selectedMeasurements);

        if (selectedMeasurements.Count == 0)
        {
            const string message = "Keine LM-/AR-Daten für die NIDEK-RT-Sendung ausgewählt.";
            return Task.FromResult(new NidekRtSerialPhoropterCommunicationResult(
                Success: false,
                ErrorMessage: message,
                PortName: settings.PortName ?? string.Empty,
                RequestBytes: RsRequestBytes,
                SentBytes: Array.Empty<byte>(),
                HandshakeBytes: Array.Empty<byte>(),
                ReceivedBytes: Array.Empty<byte>(),
                ReceivedText: string.Empty,
                ReceivedHexDump: string.Empty,
                Messages: new[] { message }));
        }

        var writerResult = _writer.BuildFrame(patient, selectedMeasurements, model);
        if (!writerResult.Success)
        {
            return Task.FromResult(new NidekRtSerialPhoropterCommunicationResult(
                Success: false,
                ErrorMessage: writerResult.ErrorMessage,
                PortName: settings.PortName ?? string.Empty,
                RequestBytes: RsRequestBytes,
                SentBytes: writerResult.Bytes,
                HandshakeBytes: Array.Empty<byte>(),
                ReceivedBytes: Array.Empty<byte>(),
                ReceivedText: string.Empty,
                ReceivedHexDump: string.Empty,
                Messages: writerResult.Warnings.Concat(new[] { writerResult.ErrorMessage ?? "NIDEK-RT-Sendedaten konnten nicht erzeugt werden." }).ToArray()));
        }

        return ExchangeAsync(settings, RsRequestBytes, ReadyToSendMarker, writerResult.Bytes, writerResult.Warnings, cancellationToken);
    }

    public Task<NidekRtSerialPhoropterCommunicationResult> ReceiveReturnAsync(
        SerialCommunicationSettings settings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return ExchangeAsync(settings, Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<string>(), cancellationToken);
    }

    public static byte[] CreateRsRequestBytes()
    {
        return RsRequestBytes.ToArray();
    }

    public static byte[] CreateReadyToSendMarker()
    {
        return ReadyToSendMarker.ToArray();
    }

    private async Task<NidekRtSerialPhoropterCommunicationResult> ExchangeAsync(
        SerialCommunicationSettings settings,
        byte[] requestBytes,
        byte[] expectedHandshakeBytes,
        byte[] payloadBytes,
        IReadOnlyList<string> writerWarnings,
        CancellationToken cancellationToken)
    {
        var exchangeRequest = new SerialCommunicationExchangeRequest(
            RequestBytes: requestBytes,
            ExpectedHandshakeBytes: expectedHandshakeBytes,
            PayloadBytes: payloadBytes,
            EndOfTransmissionByte: NidekRtSerialControlChars.ET,
            HandshakeTimeout: _options.HandshakeTimeout,
            ReceiveTimeout: _options.ReceiveTimeout,
            StableAfterEndOfTransmission: _options.StableAfterEndOfTransmission,
            MaxReceiveBytes: _options.MaxReceiveBytes);

        var exchangeResult = await _serialCommunicationService
            .ExchangeAsync(settings, exchangeRequest, cancellationToken)
            .ConfigureAwait(false);
        var messages = writerWarnings.Concat(exchangeResult.Messages).ToArray();

        return new NidekRtSerialPhoropterCommunicationResult(
            Success: exchangeResult.Success,
            ErrorMessage: exchangeResult.ErrorMessage,
            PortName: exchangeResult.PortName,
            RequestBytes: requestBytes,
            SentBytes: payloadBytes,
            HandshakeBytes: exchangeResult.HandshakeBytes,
            ReceivedBytes: exchangeResult.ReceivedBytes,
            ReceivedText: exchangeResult.RawText,
            ReceivedHexDump: exchangeResult.HexDump,
            Messages: messages);
    }
}
