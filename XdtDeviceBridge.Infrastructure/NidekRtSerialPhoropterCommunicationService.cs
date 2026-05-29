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
                Messages: new[] { message, $"COM-Einstellungen: {SerialDiagnosticsFormatter.FormatSettings(settings)}" }));
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

        return ExchangeAsync(settings, RsRequestBytes, ReadyToSendMarker, writerResult.Bytes, writerResult.Warnings, model, cancellationToken);
    }

    public Task<NidekRtSerialPhoropterCommunicationResult> ReceiveReturnAsync(
        SerialCommunicationSettings settings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return ExchangeAsync(
            settings,
            Array.Empty<byte>(),
            Array.Empty<byte>(),
            Array.Empty<byte>(),
            Array.Empty<string>(),
            NidekRtSerialPhoropterModel.Rt3100,
            cancellationToken);
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
        NidekRtSerialPhoropterModel model,
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
        var messages = CreateDiagnosticMessages(
                settings,
                requestBytes,
                expectedHandshakeBytes,
                payloadBytes,
                writerWarnings,
                exchangeResult,
                model)
            .ToArray();

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

    private static IEnumerable<string> CreateDiagnosticMessages(
        SerialCommunicationSettings settings,
        byte[] requestBytes,
        byte[] expectedHandshakeBytes,
        byte[] payloadBytes,
        IReadOnlyList<string> writerWarnings,
        SerialCommunicationExchangeResult exchangeResult,
        NidekRtSerialPhoropterModel model)
    {
        yield return $"COM-Einstellungen: {SerialDiagnosticsFormatter.FormatSettings(settings)}";

        if (requestBytes.Length > 0)
        {
            yield return $"RS-Anforderung erwartet: {SerialDiagnosticsFormatter.ToVisibleControlText(requestBytes)}";
            yield return $"RS-Anforderung Hexdump: {SerialDiagnosticsFormatter.ToHexDump(requestBytes)}";
        }
        else
        {
            yield return "Nur-Abhören aktiv: Es wird keine RS-Anforderung gesendet.";
        }

        if (expectedHandshakeBytes.Length > 0)
        {
            yield return $"Erwartete SD-Bestätigung: dokumentiert <SOH>CRL<STX>SD<ETB><EOT>, geprüft wird Marker {SerialDiagnosticsFormatter.ToVisibleControlText(expectedHandshakeBytes)}.";
        }

        if (payloadBytes.Length > 0)
        {
            yield return $"PC->RT-Writer-Frame ({CreateModelDisplayName(model)}): {payloadBytes.Length} Bytes.";
            yield return $"PC->RT-Writer sichtbar: {SerialDiagnosticsFormatter.ToVisibleControlText(payloadBytes)}";
            yield return $"PC->RT-Writer Hexdump: {SerialDiagnosticsFormatter.ToHexDump(payloadBytes)}";
            yield return $"PC->RT-Blöcke: {DescribeWriterBlocks(payloadBytes)}";
        }
        else if (requestBytes.Length > 0)
        {
            yield return "PC->RT-Writer-Frame: keine Sendedaten erzeugt.";
        }

        foreach (var warning in writerWarnings.Where(message => !string.IsNullOrWhiteSpace(message)))
        {
            yield return $"Writer-Hinweis: {warning}";
        }

        foreach (var message in exchangeResult.Messages.Where(message => !string.IsNullOrWhiteSpace(message)))
        {
            yield return message;
        }

        if (!exchangeResult.Success
            && expectedHandshakeBytes.Length > 0
            && !exchangeResult.HandshakeBytes.ContainsSequence(expectedHandshakeBytes))
        {
            yield return $"Keine SD-Bestätigung vom {CreateModelDisplayName(model)} empfangen. Bitte COM-Port, Type1/Type2, DTR/DSR/Handshake und PC-Port-Einstellung am Phoropter prüfen.";
        }

        if (!exchangeResult.Success && exchangeResult.ReceivedBytes.Length == 0)
        {
            yield return SerialDiagnosticsFormatter.CreateNoRtDataTroubleshooting(CreateModelDisplayName(model));
        }
    }

    private static string CreateModelDisplayName(NidekRtSerialPhoropterModel model)
    {
        return model switch
        {
            NidekRtSerialPhoropterModel.Rt2100 => "RT-2100",
            NidekRtSerialPhoropterModel.Rt5100 => "RT-5100",
            _ => "RT-3100"
        };
    }

    private static string DescribeWriterBlocks(byte[] payloadBytes)
    {
        var visible = SerialDiagnosticsFormatter.ToVisibleControlText(payloadBytes);
        var blocks = new List<string>();
        AddIfContains(blocks, visible, "DRL", "ID");
        AddIfContains(blocks, visible, "DRM", "AR SCA");
        AddIfContains(blocks, visible, "DWF", "WF SCA");
        AddIfContains(blocks, visible, "DLM", "LM SCA");
        AddIfContains(blocks, visible, "AR", "R ADD/AR-Zeile");
        AddIfContains(blocks, visible, "AL", "L ADD");
        AddIfContains(blocks, visible, "PD", "PD");
        AddIfContains(blocks, visible, "PR", "Prisma");

        return blocks.Count == 0 ? "keine erkannten Nutzdatenblöcke" : string.Join(", ", blocks.Distinct(StringComparer.Ordinal));
    }

    private static void AddIfContains(List<string> blocks, string text, string marker, string displayName)
    {
        if (text.Contains(marker, StringComparison.Ordinal))
        {
            blocks.Add(displayName);
        }
    }
}

file static class NidekRtSerialPhoropterCommunicationByteExtensions
{
    public static bool ContainsSequence(this byte[] buffer, byte[] expected)
    {
        if (expected.Length == 0)
        {
            return true;
        }

        if (buffer.Length < expected.Length)
        {
            return false;
        }

        for (var index = 0; index <= buffer.Length - expected.Length; index++)
        {
            var matches = true;
            for (var expectedIndex = 0; expectedIndex < expected.Length; expectedIndex++)
            {
                if (buffer[index + expectedIndex] != expected[expectedIndex])
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
            {
                return true;
            }
        }

        return false;
    }
}
