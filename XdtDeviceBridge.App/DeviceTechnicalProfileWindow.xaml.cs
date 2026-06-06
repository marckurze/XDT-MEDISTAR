using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.App;

public partial class DeviceTechnicalProfileWindow : Window
{
    private readonly DeviceTechnicalProfileService _service;
    private readonly string _deviceProfileId;

    public DeviceTechnicalProfileWindow(DeviceTechnicalProfileService service, string deviceProfileId)
    {
        ArgumentNullException.ThrowIfNull(service);

        _service = service;
        _deviceProfileId = deviceProfileId;

        InitializeComponent();
        DataContext = new DeviceTechnicalProfileViewModel(
            _service.LoadProfile(_deviceProfileId)
            ?? throw new InvalidOperationException("Geräte-Steckbrief wurde nicht gefunden."));
    }

    private DeviceTechnicalProfileViewModel ViewModel => (DeviceTechnicalProfileViewModel)DataContext;

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var overrideProfile = new DeviceTechnicalProfileOverride(
            _deviceProfileId,
            ShortDescription: ViewModel.EditableShortDescription,
            TechnicalNotes: SplitLines(ViewModel.EditableTechnicalNotes),
            TechnicianNote: ViewModel.TechnicianNote);

        _service.SaveOverride(overrideProfile);
        ViewModel.StatusMessage = "Der Original-Steckbrief wird nicht überschrieben. Ihre Änderung wurde lokal gespeichert.";
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _service.ResetOverride(_deviceProfileId);
        DataContext = new DeviceTechnicalProfileViewModel(
            _service.LoadProfile(_deviceProfileId)
            ?? throw new InvalidOperationException("Geräte-Steckbrief wurde nicht gefunden."))
        {
            StatusMessage = "Lokale Anpassung entfernt. Der Original-Steckbrief wird wieder angezeigt."
        };
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private static IReadOnlyList<string> SplitLines(string value)
    {
        return (value ?? string.Empty)
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToArray();
    }
}

public sealed class DeviceTechnicalProfileViewModel : INotifyPropertyChanged
{
    private string _editableShortDescription;
    private string _editableTechnicalNotes;
    private string _technicianNote;
    private string _statusMessage = string.Empty;

    public DeviceTechnicalProfileViewModel(DeviceTechnicalProfile profile)
    {
        DeviceProfileId = profile.DeviceProfileId;
        Manufacturer = profile.Manufacturer;
        Model = profile.Model;
        DeviceType = profile.DeviceType;
        ConnectionType = profile.ConnectionType;
        BidirectionalText = profile.IsBidirectional ? "Ja" : "Nein";
        ImagePath = profile.ImagePath;
        Measurements = profile.Measurements;
        ExampleMeasurements = profile.ExampleMeasurements ?? Array.Empty<DeviceTechnicalProfileExampleMeasurement>();
        ExampleMeasurementsVisibility = ExampleMeasurements.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        _editableShortDescription = profile.ShortDescription;
        _editableTechnicalNotes = string.Join(Environment.NewLine, profile.TechnicalNotes ?? Array.Empty<string>());
        _technicianNote = profile.TechnicianNote;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string DeviceProfileId { get; }

    public string Manufacturer { get; }

    public string Model { get; }

    public string DeviceType { get; }

    public string ConnectionType { get; }

    public string BidirectionalText { get; }

    public string ImagePath { get; }

    public IReadOnlyList<DeviceTechnicalProfileMeasurement> Measurements { get; }

    public IReadOnlyList<DeviceTechnicalProfileExampleMeasurement> ExampleMeasurements { get; }

    public Visibility ExampleMeasurementsVisibility { get; }

    public string EditableShortDescription
    {
        get => _editableShortDescription;
        set => SetField(ref _editableShortDescription, value);
    }

    public string EditableTechnicalNotes
    {
        get => _editableTechnicalNotes;
        set => SetField(ref _editableTechnicalNotes, value);
    }

    public string TechnicianNote
    {
        get => _technicianNote;
        set => SetField(ref _technicianNote, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    private void SetField(ref string field, string value, [CallerMemberName] string? propertyName = null)
    {
        if (string.Equals(field, value, StringComparison.Ordinal))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
