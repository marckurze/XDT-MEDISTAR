using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.App;

public partial class LoadDeviceProfileDialog : Window
{
    private readonly IReadOnlyList<DeviceProfileDefinition> _deviceProfiles;
    private readonly Dictionary<string, DeviceProfileDefinition> _deviceProfilesById;
    private readonly AppDataPaths _paths;
    private readonly DeviceProfileImageOverrideService _imageOverrideService;

    private string? _pendingImagePath;
    private bool _pendingReset;

    public LoadDeviceProfileDialog(
        IEnumerable<DeviceProfileDefinition> deviceProfiles,
        AppDataPaths paths,
        DeviceProfileImageOverrideService imageOverrideService)
    {
        ArgumentNullException.ThrowIfNull(deviceProfiles);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(imageOverrideService);

        InitializeComponent();

        _deviceProfiles = deviceProfiles
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(profile => profile.Metadata.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();
        _deviceProfilesById = _deviceProfiles.ToDictionary(profile => profile.Metadata.Id, StringComparer.OrdinalIgnoreCase);
        _paths = paths;
        _imageOverrideService = imageOverrideService;

        RefreshDeviceList(selectedDeviceProfileId: null);
        Loaded += (_, _) => DeviceProfilesGrid.Focus();
    }

    public bool HasChanges { get; private set; }

    private void RefreshDeviceList(string? selectedDeviceProfileId)
    {
        var items = _imageOverrideService.BuildManagementItems(_paths, _deviceProfiles);
        DeviceProfilesGrid.ItemsSource = items;

        var selectedItem = !string.IsNullOrWhiteSpace(selectedDeviceProfileId)
            ? items.FirstOrDefault(item => string.Equals(item.DeviceProfileId, selectedDeviceProfileId, StringComparison.OrdinalIgnoreCase))
            : items.FirstOrDefault();

        DeviceProfilesGrid.SelectedItem = selectedItem;
        if (selectedItem is null)
        {
            ClearDetails();
        }
    }

    private void DeviceProfilesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _pendingImagePath = null;
        _pendingReset = false;

        if (DeviceProfilesGrid.SelectedItem is not DeviceProfileImageManagementItem item)
        {
            ClearDetails();
            return;
        }

        ProfileNameText.Text = item.ProfileName;
        ManufacturerText.Text = item.Manufacturer;
        ModelText.Text = item.Model;
        DeviceTypeText.Text = item.DeviceType;
        ParserModeText.Text = item.ParserMode;
        ProfileKindText.Text = item.IsBuiltIn
            ? $"BuiltIn; bidirektional: {FormatYesNo(item.IsBidirectional)}; fachliche Werte schreibgeschützt"
            : $"UserDefined; bidirektional: {FormatYesNo(item.IsBidirectional)}";

        SetPreviewImage(item.EffectiveImagePath);
        ImageStatusText.Text = item.HasLocalOverride
            ? $"Lokaler Bild-Override aktiv: {item.EffectiveImagePath}"
            : string.IsNullOrWhiteSpace(item.EffectiveImagePath)
                ? "Kein Gerätebild hinterlegt. Die Monitoring-Karte zeigt den Platzhalter."
                : $"Bild aus Profil oder BuiltIn-Asset: {item.EffectiveImagePath}";
    }

    private void SelectImage_Click(object sender, RoutedEventArgs e)
    {
        if (DeviceProfilesGrid.SelectedItem is not DeviceProfileImageManagementItem)
        {
            System.Windows.MessageBox.Show(this, "Bitte zuerst ein Geräteprofil auswählen.", "Gerät laden", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Gerätebild auswählen",
            Filter = "Bilddateien (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Alle Dateien (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        _pendingImagePath = dialog.FileName;
        _pendingReset = false;
        SetPreviewImage(dialog.FileName);
        ImageStatusText.Text = "Ausgewähltes Bild wird beim Speichern in den lokalen AppData-Bildordner kopiert.";
    }

    private void ResetImage_Click(object sender, RoutedEventArgs e)
    {
        if (DeviceProfilesGrid.SelectedItem is not DeviceProfileImageManagementItem item)
        {
            System.Windows.MessageBox.Show(this, "Bitte zuerst ein Geräteprofil auswählen.", "Gerät laden", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _pendingImagePath = null;
        _pendingReset = true;

        var fallbackImagePath = _deviceProfilesById.TryGetValue(item.DeviceProfileId, out var profile)
            ? _imageOverrideService.ResolveEffectiveImagePath(profile, overridePath: null)
            : string.Empty;
        SetPreviewImage(fallbackImagePath);
        ImageStatusText.Text = string.IsNullOrWhiteSpace(fallbackImagePath)
            ? "Lokaler Bild-Override wird beim Speichern entfernt. Danach wird der Platzhalter angezeigt."
            : "Lokaler Bild-Override wird beim Speichern entfernt. Danach gilt wieder das Profil- oder BuiltIn-Bild.";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DeviceProfilesGrid.SelectedItem is not DeviceProfileImageManagementItem item)
        {
            System.Windows.MessageBox.Show(this, "Bitte zuerst ein Geräteprofil auswählen.", "Gerät laden", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (_pendingReset)
        {
            _imageOverrideService.RemoveImageOverride(_paths, item.DeviceProfileId);
            HasChanges = true;
            RefreshDeviceList(item.DeviceProfileId);
            ImageStatusText.Text = "Gerätebild zurückgesetzt.";
            return;
        }

        if (!string.IsNullOrWhiteSpace(_pendingImagePath))
        {
            var result = _imageOverrideService.SaveImageOverride(_paths, item.DeviceProfileId, _pendingImagePath);
            if (!result.Success)
            {
                System.Windows.MessageBox.Show(
                    this,
                    result.ErrorMessage ?? "Gerätebild konnte nicht gespeichert werden.",
                    "Gerät laden",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            HasChanges = true;
            RefreshDeviceList(item.DeviceProfileId);
            ImageStatusText.Text = $"Gerätebild gespeichert: {result.ImagePath}";
            return;
        }

        ImageStatusText.Text = "Keine Bildänderung zum Speichern vorgemerkt.";
    }

    private void ClearDetails()
    {
        ProfileNameText.Text = "";
        ManufacturerText.Text = "";
        ModelText.Text = "";
        DeviceTypeText.Text = "";
        ParserModeText.Text = "";
        ProfileKindText.Text = "";
        ImageStatusText.Text = "Kein Geräteprofil ausgewählt.";
        SetPreviewImage(string.Empty);
    }

    private void SetPreviewImage(string? imagePath)
    {
        DeviceImagePreview.Source = null;
        DeviceImagePlaceholderText.Visibility = Visibility.Visible;

        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return;
        }

        try
        {
            var trimmed = imagePath.Trim();
            var uri = trimmed.StartsWith("pack://", StringComparison.OrdinalIgnoreCase)
                ? new Uri(trimmed, UriKind.Absolute)
                : new Uri(trimmed, UriKind.RelativeOrAbsolute);

            if (!trimmed.StartsWith("pack://", StringComparison.OrdinalIgnoreCase)
                && Path.IsPathFullyQualified(trimmed)
                && !File.Exists(trimmed))
            {
                return;
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = uri;
            bitmap.EndInit();
            bitmap.Freeze();

            DeviceImagePreview.Source = bitmap;
            DeviceImagePlaceholderText.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or NotSupportedException or UriFormatException)
        {
            DeviceImagePreview.Source = null;
            DeviceImagePlaceholderText.Visibility = Visibility.Visible;
        }
    }

    private static string FormatYesNo(bool value)
    {
        return value ? "Ja" : "Nein";
    }
}
