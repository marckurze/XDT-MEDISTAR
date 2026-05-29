using System.Text;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class NidekRtSerialPhoropterCommunicationServiceTests
{
    [Fact]
    public async Task SendSelectionAndReceiveAsync_ShouldSendRsHandshakeThenPayloadAndCollectReturn()
    {
        var fakeSerial = new FakeSerialDeviceCommunicationService
        {
            ExchangeResult = CreateExchangeResult(success: true, receivedBytes: CreatePracticeReturnBytes())
        };
        var service = new NidekRtSerialPhoropterCommunicationService(fakeSerial);

        var result = await service.SendSelectionAndReceiveAsync(
            NidekRs232CommunicationPresets.CreateRt3100Type1Preset("COM7"),
            CreatePatient(),
            CreateHistoricalRecords(),
            NidekRtSerialPhoropterModel.Rt3100,
            CancellationToken.None);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.NotNull(fakeSerial.LastExchangeRequest);
        Assert.Equal(NidekRtSerialPhoropterCommunicationService.CreateRsRequestBytes(), fakeSerial.LastExchangeRequest.RequestBytes);
        Assert.Equal(NidekRtSerialPhoropterCommunicationService.CreateReadyToSendMarker(), fakeSerial.LastExchangeRequest.ExpectedHandshakeBytes);
        Assert.NotEmpty(fakeSerial.LastExchangeRequest.PayloadBytes);
        Assert.Equal(NidekRtSerialControlChars.SH, fakeSerial.LastExchangeRequest.PayloadBytes.First());
        Assert.Equal(NidekRtSerialControlChars.ET, fakeSerial.LastExchangeRequest.PayloadBytes.Last());
        Assert.Equal(NidekRtSerialControlChars.ET, fakeSerial.LastExchangeRequest.EndOfTransmissionByte);
        Assert.Equal(TimeSpan.FromMilliseconds(800), fakeSerial.LastExchangeRequest.StableAfterEndOfTransmission);
        Assert.Contains("DRL", Encoding.ASCII.GetString(fakeSerial.LastExchangeRequest.PayloadBytes), StringComparison.Ordinal);
        Assert.Equal(CreatePracticeReturnBytes(), result.ReceivedBytes);
        Assert.NotNull(fakeSerial.LastSettings);
        Assert.True(fakeSerial.LastSettings!.DtrEnable);
        Assert.True(fakeSerial.LastSettings.RtsEnable);
        Assert.Contains(result.Messages, message => message.Contains("RS-Anforderung Hexdump", StringComparison.Ordinal));
        Assert.Contains(result.Messages, message => message.Contains("01 43 20 20 20 02 52 53 17 04", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Messages, message => message.Contains("01 43 20 2A 2A 02 52 53 17 04", StringComparison.Ordinal));
        Assert.Contains(result.Messages, message => message.Contains("PC->RT-Writer Hexdump", StringComparison.Ordinal));
        Assert.Contains(result.Messages, message => message.Contains("PC->RT-Blöcke", StringComparison.Ordinal) && message.Contains("LM ADD", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Messages, message => message.Contains("R ADD/AR-Zeile", StringComparison.Ordinal));
    }

    [Fact]
    public void CreateRsRequestBytes_ShouldUseSpacePlaceholdersInsteadOfAsterisks()
    {
        var request = NidekRtSerialPhoropterCommunicationService.CreateRsRequestBytes();

        Assert.Equal(
            new byte[]
            {
                0x01, 0x43, 0x20, 0x20, 0x20, 0x02, 0x52, 0x53, 0x17, 0x04
            },
            request);
        Assert.DoesNotContain((byte)'*', request);
        Assert.Equal("<SOH>C   <STX>RS<ETB><EOT>", SerialDiagnosticsFormatter.ToVisibleControlText(request));
        Assert.Equal("01 43 20 20 20 02 52 53 17 04", SerialDiagnosticsFormatter.ToHexDump(request));
    }

    [Fact]
    public async Task ReceiveReturnAsync_ShouldListenWithoutHandshakeOrPayload()
    {
        var fakeSerial = new FakeSerialDeviceCommunicationService
        {
            ExchangeResult = CreateExchangeResult(success: true, receivedBytes: CreatePracticeReturnBytes())
        };
        var service = new NidekRtSerialPhoropterCommunicationService(fakeSerial);

        var result = await service.ReceiveReturnAsync(
            NidekRs232CommunicationPresets.CreateRt3100Type1Preset("COM7"),
            CancellationToken.None);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.NotNull(fakeSerial.LastExchangeRequest);
        Assert.Empty(fakeSerial.LastExchangeRequest.RequestBytes);
        Assert.Empty(fakeSerial.LastExchangeRequest.ExpectedHandshakeBytes);
        Assert.Empty(fakeSerial.LastExchangeRequest.PayloadBytes);
        Assert.Equal(NidekRtSerialControlChars.ET, fakeSerial.LastExchangeRequest.EndOfTransmissionByte);
        Assert.Contains("NIDEK RT-3100", result.ReceivedText, StringComparison.Ordinal);
        Assert.Contains(result.Messages, message => message.Contains("Nur-Abhören aktiv", StringComparison.Ordinal));
        Assert.Contains(result.Messages, message => message.Contains("COM-Einstellungen", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SendSelectionAndReceiveAsync_ShouldReturnFriendlyErrorWhenWriterHasNoLmOrArData()
    {
        var fakeSerial = new FakeSerialDeviceCommunicationService();
        var service = new NidekRtSerialPhoropterCommunicationService(fakeSerial);

        var result = await service.SendSelectionAndReceiveAsync(
            NidekRs232CommunicationPresets.CreateRt3100Type1Preset("COM7"),
            CreatePatient(),
            Array.Empty<AisHistoricalMeasurementRecord>(),
            NidekRtSerialPhoropterModel.Rt3100,
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("LM-/AR-Daten", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Null(fakeSerial.LastExchangeRequest);
    }

    [Fact]
    public async Task SendSelectionAndReceiveAsync_ShouldSurfaceMissingSdConfirmation()
    {
        var fakeSerial = new FakeSerialDeviceCommunicationService
        {
            ExchangeResult = CreateExchangeResult(
                success: false,
                receivedBytes: Array.Empty<byte>(),
                errorMessage: "Keine RT-Antwort auf RS-Anforderung empfangen oder SD-Bestätigung fehlt.")
        };
        var service = new NidekRtSerialPhoropterCommunicationService(fakeSerial);

        var result = await service.SendSelectionAndReceiveAsync(
            NidekRs232CommunicationPresets.CreateRt3100Type1Preset("COM7"),
            CreatePatient(),
            CreateHistoricalRecords(),
            NidekRtSerialPhoropterModel.Rt3100,
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("SD-Bestätigung", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(fakeSerial.LastExchangeRequest);
        Assert.Contains(result.Messages, message => message.Contains("Keine SD-Bestätigung vom RT-3100", StringComparison.Ordinal));
        Assert.Contains(result.Messages, message => message.Contains("PC-Port-Einstellung", StringComparison.Ordinal));
        Assert.Contains(result.Messages, message => message.Contains("PRINT/SEND", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SendSelectionAndReceiveAsync_ShouldExposeBytesWithoutSdConfirmation()
    {
        var fakeSerial = new FakeSerialDeviceCommunicationService
        {
            ExchangeResult = CreateExchangeResult(
                success: false,
                receivedBytes: Array.Empty<byte>(),
                errorMessage: "Keine RT-Antwort auf RS-Anforderung empfangen oder SD-Bestätigung fehlt.",
                handshakeBytes: Encoding.ASCII.GetBytes("NACK"))
        };
        var service = new NidekRtSerialPhoropterCommunicationService(fakeSerial);

        var result = await service.SendSelectionAndReceiveAsync(
            NidekRs232CommunicationPresets.CreateRt3100Type1Preset("COM7"),
            CreatePatient(),
            CreateHistoricalRecords(),
            NidekRtSerialPhoropterModel.Rt3100,
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(Encoding.ASCII.GetBytes("NACK"), result.HandshakeBytes);
        Assert.Contains(result.Messages, message => message.Contains("Bytes empfangen, aber keine SD-Bestätigung erkannt", StringComparison.Ordinal));
        Assert.Contains(result.Messages, message => message.Contains("NACK", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SendSelectionAndReceiveAsync_ShouldWarnWhenRt3100DtrIsDisabled()
    {
        var fakeSerial = new FakeSerialDeviceCommunicationService
        {
            ExchangeResult = CreateExchangeResult(
                success: false,
                receivedBytes: Array.Empty<byte>(),
                errorMessage: "Keine RT-Antwort auf RS-Anforderung empfangen oder SD-Bestätigung fehlt.")
        };
        var service = new NidekRtSerialPhoropterCommunicationService(fakeSerial);

        var result = await service.SendSelectionAndReceiveAsync(
            NidekRs232CommunicationPresets.CreateRt3100Type1Preset("COM7") with { DtrEnable = false },
            CreatePatient(),
            CreateHistoricalRecords(),
            NidekRtSerialPhoropterModel.Rt3100,
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains(result.Messages, message => message.Contains("letzten erfolgreichen RT-3100-Abhören war DTR aktiv", StringComparison.Ordinal));
    }

    private static SerialCommunicationExchangeResult CreateExchangeResult(
        bool success,
        byte[] receivedBytes,
        string? errorMessage = null,
        byte[]? handshakeBytes = null)
    {
        return new SerialCommunicationExchangeResult(
            Success: success,
            ErrorMessage: errorMessage,
            PortName: "COM7",
            BytesWritten: 42,
            HandshakeBytes: handshakeBytes ?? (success
                ? NidekRtSerialPhoropterCommunicationService.CreateReadyToSendMarker()
                : Array.Empty<byte>()),
            ReceivedBytes: receivedBytes,
            RawText: Encoding.ASCII.GetString(receivedBytes),
            HexDump: string.Join(" ", receivedBytes.Select(value => value.ToString("X2"))),
            Messages: string.IsNullOrWhiteSpace(errorMessage)
                ? new[] { "Rückgabe vollständig empfangen." }
                : CreateExchangeMessages(errorMessage, handshakeBytes));
    }

    private static IReadOnlyList<string> CreateExchangeMessages(string errorMessage, byte[]? handshakeBytes)
    {
        if (handshakeBytes is null || handshakeBytes.Length == 0)
        {
            return new[] { errorMessage, "Keine Bytes empfangen.", "Empfangene SD-Antwort: keine Bytes." };
        }

        return new[]
        {
            errorMessage,
            "Bytes empfangen, aber keine SD-Bestätigung erkannt.",
            $"Empfangene SD-Antwort: {SerialDiagnosticsFormatter.ToVisibleControlText(handshakeBytes)}",
            $"SD-Hexdump: {SerialDiagnosticsFormatter.ToHexDump(handshakeBytes)}"
        };
    }

    private static PatientData CreatePatient()
    {
        return new PatientData(
            PatientNumber: "4701-1",
            LastName: "Testfrau",
            FirstName: "Anna",
            BirthDate: "12061955",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: "Phoro");
    }

    private static IReadOnlyList<AisHistoricalMeasurementRecord> CreateHistoricalRecords()
    {
        return new[]
        {
            new AisHistoricalMeasurementRecord(
                new DateOnly(2026, 5, 29),
                "V0",
                AisHistoricalMeasurementSourceKind.Lensmeter,
                null,
                Array.Empty<string>(),
                new AisHistoricalEyeRefraction("+6.25", "-3.25", "3", "+1.50"),
                new AisHistoricalEyeRefraction("+6.50", "-2.75", "170", "+1.50"),
                "65",
                null,
                true,
                Array.Empty<string>()),
            new AisHistoricalMeasurementRecord(
                new DateOnly(2026, 5, 29),
                "V1",
                AisHistoricalMeasurementSourceKind.Autorefraction,
                null,
                Array.Empty<string>(),
                new AisHistoricalEyeRefraction("+1.00", "-1.25", "7", null),
                new AisHistoricalEyeRefraction("+1.50", "-0.75", "90", null),
                "64",
                null,
                true,
                Array.Empty<string>())
        };
    }

    private static byte[] CreatePracticeReturnBytes()
    {
        return new byte[]
        {
            0x01, 0x4E, 0x49, 0x44, 0x45, 0x4B, 0x20, 0x52, 0x54, 0x2D, 0x33, 0x31, 0x30, 0x30,
            0x20, 0x49, 0x44, 0x20, 0x20, 0x20, 0x20, 0x44, 0x41, 0x32, 0x30, 0x30, 0x32,
            0x2F, 0x30, 0x36, 0x2F, 0x31, 0x36, 0x0D, 0x02, 0x40, 0x52, 0x54, 0x0D,
            0x02, 0x46, 0x52, 0x2D, 0x20, 0x31, 0x2E, 0x35, 0x30, 0x2D, 0x20, 0x30,
            0x2E, 0x37, 0x35, 0x31, 0x38, 0x30, 0x0D, 0x04, 0x0D
        };
    }

    private sealed class FakeSerialDeviceCommunicationService : ISerialDeviceCommunicationService
    {
        public SerialCommunicationExchangeRequest? LastExchangeRequest { get; private set; }

        public SerialCommunicationSettings? LastSettings { get; private set; }

        public SerialCommunicationExchangeResult? ExchangeResult { get; set; }

        public Task<SerialCommunicationSessionResult> ListenAsync(
            SerialCommunicationSettings settings,
            TimeSpan duration,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<SerialCommunicationWriteResult> WriteAsync(
            SerialCommunicationSettings settings,
            string command,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<SerialCommunicationExchangeResult> ExchangeAsync(
            SerialCommunicationSettings settings,
            SerialCommunicationExchangeRequest request,
            CancellationToken cancellationToken)
        {
            LastSettings = settings;
            LastExchangeRequest = request;
            return Task.FromResult(ExchangeResult ?? CreateExchangeResult(success: true, receivedBytes: Array.Empty<byte>()));
        }
    }
}
