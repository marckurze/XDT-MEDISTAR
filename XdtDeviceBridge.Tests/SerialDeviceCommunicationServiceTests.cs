using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class SerialDeviceCommunicationServiceTests
{
    [Fact]
    public void ValidateSettings_ShouldAcceptDefaultSettingsWhenPortIsNotRequired()
    {
        var issue = SerialDeviceCommunicationService.ValidateSettings(
            SerialCommunicationSettings.Default,
            requirePortName: false);

        Assert.Null(issue);
    }

    [Fact]
    public void SerialDiagnosticsFormatter_ShouldExposeControlLinesAndTimeouts()
    {
        var settings = SerialCommunicationSettings.Default with
        {
            PortName = "COM7",
            BaudRate = 2400,
            DataBits = 7,
            StopBits = SerialStopBitsSetting.Two,
            Parity = SerialParitySetting.Even,
            Handshake = SerialHandshakeSetting.RequestToSend,
            DtrEnable = true,
            RtsEnable = false,
            ReadTimeoutMilliseconds = 1500,
            WriteTimeoutMilliseconds = 2500
        };

        var display = SerialDiagnosticsFormatter.FormatSettings(settings);

        Assert.Contains("COM7", display, StringComparison.Ordinal);
        Assert.Contains("2400 Baud", display, StringComparison.Ordinal);
        Assert.Contains("7 Datenbits", display, StringComparison.Ordinal);
        Assert.Contains("Even", display, StringComparison.Ordinal);
        Assert.Contains("RequestToSend", display, StringComparison.Ordinal);
        Assert.Contains("DTR aktiv", display, StringComparison.Ordinal);
        Assert.Contains("RTS aus", display, StringComparison.Ordinal);
        Assert.Contains("ReadTimeout 1500 ms", display, StringComparison.Ordinal);
        Assert.Contains("WriteTimeout 2500 ms", display, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateSettings_ShouldReportMissingPortWhenRequired()
    {
        var issue = SerialDeviceCommunicationService.ValidateSettings(
            SerialCommunicationSettings.Default,
            requirePortName: true);

        Assert.Equal("Bitte einen COM-Port auswählen.", issue);
    }

    [Theory]
    [InlineData(0, 8, "Baudrate muss größer als 0 sein.")]
    [InlineData(9600, 4, "Datenbits müssen zwischen 5 und 8 liegen.")]
    [InlineData(9600, 9, "Datenbits müssen zwischen 5 und 8 liegen.")]
    public void ValidateSettings_ShouldReportInvalidNumericParameters(
        int baudRate,
        int dataBits,
        string expectedIssue)
    {
        var settings = SerialCommunicationSettings.Default with
        {
            PortName = "COM1",
            BaudRate = baudRate,
            DataBits = dataBits
        };

        var issue = SerialDeviceCommunicationService.ValidateSettings(settings, requirePortName: true);

        Assert.Equal(expectedIssue, issue);
    }

    [Fact]
    public async Task ListenAsync_ShouldReturnFriendlyErrorForMissingPortName()
    {
        var service = new SerialDeviceCommunicationService();

        var result = await service.ListenAsync(
            SerialCommunicationSettings.Default,
            TimeSpan.FromMilliseconds(50),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Bitte einen COM-Port auswählen.", result.ErrorMessage);
        Assert.Equal(0, result.BytesReceived);
    }

    [Fact]
    public async Task ListenAsync_ShouldReturnCanceledResultWithoutOpeningPort()
    {
        var service = new SerialDeviceCommunicationService();
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var result = await service.ListenAsync(
            SerialCommunicationSettings.Default with { PortName = "COM1" },
            TimeSpan.FromMilliseconds(50),
            cancellationTokenSource.Token);

        Assert.False(result.Success);
        Assert.Equal("Vorgang wurde abgebrochen.", result.ErrorMessage);
        Assert.Equal(0, result.BytesReceived);
    }

    [Fact]
    public async Task WriteAsync_ShouldReturnFriendlyErrorForMissingPortName()
    {
        var service = new SerialDeviceCommunicationService();

        var result = await service.WriteAsync(
            SerialCommunicationSettings.Default,
            "TEST",
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Bitte einen COM-Port auswählen.", result.ErrorMessage);
        Assert.Equal(0, result.BytesWritten);
    }

    [Fact]
    public async Task ExchangeAsync_ShouldReturnFriendlyErrorForMissingPortName()
    {
        var service = new SerialDeviceCommunicationService();
        var request = new SerialCommunicationExchangeRequest(
            RequestBytes: Array.Empty<byte>(),
            ExpectedHandshakeBytes: Array.Empty<byte>(),
            PayloadBytes: Array.Empty<byte>(),
            EndOfTransmissionByte: 0x04,
            HandshakeTimeout: TimeSpan.FromMilliseconds(50),
            ReceiveTimeout: TimeSpan.FromMilliseconds(50),
            StableAfterEndOfTransmission: TimeSpan.FromMilliseconds(10),
            MaxReceiveBytes: 1024);

        var result = await service.ExchangeAsync(
            SerialCommunicationSettings.Default,
            request,
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Bitte einen COM-Port auswählen.", result.ErrorMessage);
        Assert.Empty(result.ReceivedBytes);
    }

    [Fact]
    public void SerialPortDiscoveryService_ShouldReturnListWithoutThrowing()
    {
        var service = new SerialPortDiscoveryService();

        var ports = service.GetAvailablePortNames();

        Assert.NotNull(ports);
    }
}
