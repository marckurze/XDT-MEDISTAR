using System.Text;
using System.Windows;
using System.IO;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.App;

public partial class XdtBaukastenSerialCaptureWindow : Window
{
    private readonly ISerialPortDiscoveryService _portDiscoveryService;
    private readonly ISerialDeviceCommunicationService _communicationService;
    private CancellationTokenSource? _listenCancellationTokenSource;
    private SerialCommunicationSessionResult? _lastResult;

    public XdtBaukastenSerialCaptureWindow(
        ISerialPortDiscoveryService portDiscoveryService,
        ISerialDeviceCommunicationService communicationService,
        SerialCommunicationSettings? defaultSettings = null)
    {
        _portDiscoveryService = portDiscoveryService ?? throw new ArgumentNullException(nameof(portDiscoveryService));
        _communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));

        InitializeComponent();
        ParityComboBox.ItemsSource = Enum.GetValues<SerialParitySetting>();
        StopBitsComboBox.ItemsSource = Enum.GetValues<SerialStopBitsSetting>();
        HandshakeComboBox.ItemsSource = Enum.GetValues<SerialHandshakeSetting>();
        LineTerminatorComboBox.ItemsSource = Enum.GetValues<SerialLineTerminatorSetting>();
        InitializeFromSettings(defaultSettings ?? SerialCommunicationSettings.Default);
        RefreshPorts();
    }

    public SerialRawDeviceInput? CapturedInput { get; private set; }

    private void InitializeFromSettings(SerialCommunicationSettings settings)
    {
        BaudRateTextBox.Text = settings.BaudRate.ToString();
        DataBitsTextBox.Text = settings.DataBits.ToString();
        ParityComboBox.SelectedItem = settings.Parity;
        StopBitsComboBox.SelectedItem = settings.StopBits;
        HandshakeComboBox.SelectedItem = settings.Handshake;
        LineTerminatorComboBox.SelectedItem = settings.LineTerminator;
        if (!string.IsNullOrWhiteSpace(settings.PortName))
        {
            PortComboBox.Text = settings.PortName;
        }
    }

    private void RefreshPorts_Click(object sender, RoutedEventArgs e)
    {
        RefreshPorts();
    }

    private void RefreshPorts()
    {
        var selected = PortComboBox.Text;
        var ports = _portDiscoveryService.GetAvailablePortNames();
        PortComboBox.ItemsSource = ports;

        if (!string.IsNullOrWhiteSpace(selected))
        {
            PortComboBox.Text = selected;
        }
        else if (ports.Count > 0)
        {
            PortComboBox.SelectedIndex = 0;
        }

        StatusText.Text = ports.Count == 0
            ? "Kein COM-Port gefunden. Sie können den Portnamen bei Bedarf manuell eintragen."
            : $"{ports.Count} COM-Port(s) gefunden.";
    }

    private async void Listen_Click(object sender, RoutedEventArgs e)
    {
        if (!TryReadSettings(out var settings, out var duration, out var message))
        {
            StatusText.Text = message;
            return;
        }

        _listenCancellationTokenSource?.Cancel();
        _listenCancellationTokenSource?.Dispose();
        _listenCancellationTokenSource = new CancellationTokenSource();

        ListenButton.IsEnabled = false;
        StatusText.Text = $"Höre {settings.PortName} für {duration.TotalSeconds:0} Sekunden ab ...";
        try
        {
            _lastResult = await _communicationService.ListenAsync(settings, duration, _listenCancellationTokenSource.Token);
            RawTextBox.Text = _lastResult.RawText;
            HexDumpTextBox.Text = _lastResult.HexDump;
            StatusText.Text = _lastResult.Success
                ? $"{_lastResult.BytesReceived} Byte empfangen."
                : _lastResult.ErrorMessage ?? "Keine Daten empfangen.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            StatusText.Text = $"COM-Port konnte nicht gelesen werden: {ex.Message}";
        }
        finally
        {
            ListenButton.IsEnabled = true;
        }
    }

    private void Accept_Click(object sender, RoutedEventArgs e)
    {
        if (_lastResult is null)
        {
            StatusText.Text = "Bitte zuerst Daten abhören oder empfangenen Rohtext einfügen.";
            return;
        }

        if (!TryReadSettings(out var settings, out _, out var message))
        {
            StatusText.Text = message;
            return;
        }

        CapturedInput = new SerialRawDeviceInput(
            PortName: _lastResult.PortName,
            Settings: settings,
            ReceivedAt: _lastResult.FinishedAt,
            Bytes: Encoding.ASCII.GetBytes(_lastResult.RawText),
            RawText: _lastResult.RawText,
            HexDump: _lastResult.HexDump);
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        _listenCancellationTokenSource?.Cancel();
        DialogResult = false;
    }

    private bool TryReadSettings(
        out SerialCommunicationSettings settings,
        out TimeSpan duration,
        out string message)
    {
        duration = TimeSpan.Zero;
        settings = SerialCommunicationSettings.Default;

        var portName = PortComboBox.Text.Trim();
        if (!int.TryParse(BaudRateTextBox.Text.Trim(), out var baudRate))
        {
            message = "Baudrate muss eine Zahl sein.";
            return false;
        }

        if (!int.TryParse(DataBitsTextBox.Text.Trim(), out var dataBits))
        {
            message = "Datenbits müssen eine Zahl sein.";
            return false;
        }

        if (!int.TryParse(DurationSecondsTextBox.Text.Trim(), out var seconds))
        {
            message = "Dauer muss eine Zahl sein.";
            return false;
        }

        settings = new SerialCommunicationSettings(
            PortName: portName,
            BaudRate: baudRate,
            DataBits: dataBits,
            StopBits: StopBitsComboBox.SelectedItem is SerialStopBitsSetting stopBits ? stopBits : SerialStopBitsSetting.One,
            Parity: ParityComboBox.SelectedItem is SerialParitySetting parity ? parity : SerialParitySetting.None,
            Handshake: HandshakeComboBox.SelectedItem is SerialHandshakeSetting handshake ? handshake : SerialHandshakeSetting.None,
            LineTerminator: LineTerminatorComboBox.SelectedItem is SerialLineTerminatorSetting terminator ? terminator : SerialLineTerminatorSetting.CRLF);
        duration = TimeSpan.FromSeconds(seconds);
        message = string.Empty;

        var validation = SerialDeviceCommunicationService.ValidateSettings(settings, requirePortName: true);
        if (validation is not null)
        {
            message = validation;
            return false;
        }

        if (duration <= TimeSpan.Zero)
        {
            message = "Dauer muss größer als 0 Sekunden sein.";
            return false;
        }

        return true;
    }
}
