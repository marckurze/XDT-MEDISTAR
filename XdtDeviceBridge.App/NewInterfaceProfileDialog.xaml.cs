using System.Windows;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.App;

public partial class NewInterfaceProfileDialog : Window
{
    private readonly ProfileCatalog _catalog;

    public NewInterfaceProfileDialog(ProfileCatalog catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        InitializeComponent();

        InitializeProfileLists();
        ProfileNameTextBox.Text = UserDefinedProfileCreationService.CreateAvailableProfileName(
            _catalog.InterfaceProfiles.Select(profile => profile.Metadata.Name),
            "Neues Schnittstellenprofil");

        Loaded += (_, _) =>
        {
            ProfileNameTextBox.Focus();
            ProfileNameTextBox.SelectAll();
        };
    }

    public UserDefinedInterfaceProfileCreationRequest Request { get; private set; } =
        new(string.Empty, string.Empty, string.Empty, string.Empty);

    private void InitializeProfileLists()
    {
        var aisProfiles = _catalog.AisProfiles
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(profile => new ProfileSelectionItem(
                profile.Metadata.Id,
                $"{profile.Name} ({CreateProfileOriginLabel(profile.Metadata)})"))
            .ToList();

        var deviceProfiles = _catalog.DeviceProfiles
            .OrderBy(profile => profile.Manufacturer, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(profile => profile.Model, StringComparer.CurrentCultureIgnoreCase)
            .Select(profile => new ProfileSelectionItem(
                profile.Metadata.Id,
                $"{profile.Manufacturer} {profile.Model} - {profile.DeviceType} ({CreateProfileOriginLabel(profile.Metadata)})"))
            .ToList();

        var exportProfiles = _catalog.ExportProfiles
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(profile => new ProfileSelectionItem(
                profile.Metadata.Id,
                $"{profile.Metadata.Name} ({CreateProfileOriginLabel(profile.Metadata)})"))
            .ToList();

        AisProfileComboBox.ItemsSource = aisProfiles;
        DeviceProfileComboBox.ItemsSource = deviceProfiles;
        ExportProfileComboBox.ItemsSource = exportProfiles;

        AisProfileComboBox.SelectedValue = aisProfiles
            .FirstOrDefault(item => item.Id.Equals("ais-medistar-default", StringComparison.OrdinalIgnoreCase))?.Id
            ?? aisProfiles.FirstOrDefault()?.Id;
        DeviceProfileComboBox.SelectedValue = deviceProfiles.FirstOrDefault()?.Id;
        ExportProfileComboBox.SelectedValue = exportProfiles.FirstOrDefault()?.Id;
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        var profileName = ProfileNameTextBox.Text.Trim();
        var aisProfileId = AisProfileComboBox.SelectedValue as string ?? string.Empty;
        var deviceProfileId = DeviceProfileComboBox.SelectedValue as string ?? string.Empty;
        var exportProfileId = ExportProfileComboBox.SelectedValue as string ?? string.Empty;
        var issues = ValidateSelection(profileName, aisProfileId, deviceProfileId, exportProfileId);

        if (issues.Count > 0)
        {
            System.Windows.MessageBox.Show(
                this,
                string.Join(Environment.NewLine, issues),
                "Neues Schnittstellenprofil anlegen",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        Request = new UserDefinedInterfaceProfileCreationRequest(
            profileName,
            aisProfileId,
            deviceProfileId,
            exportProfileId);
        DialogResult = true;
    }

    private IReadOnlyList<string> ValidateSelection(
        string profileName,
        string aisProfileId,
        string deviceProfileId,
        string exportProfileId)
    {
        var issues = new List<string>();
        if (string.IsNullOrWhiteSpace(profileName))
        {
            issues.Add("Bitte geben Sie einen Profilnamen ein.");
        }
        else if (UserDefinedProfileCreationService.HasProfileNameOrIdConflict(
            _catalog.InterfaceProfiles.Select(profile => profile.Metadata),
            profileName))
        {
            issues.Add("Es existiert bereits ein Schnittstellenprofil mit diesem Namen oder dieser ID.");
        }

        if (string.IsNullOrWhiteSpace(aisProfileId))
        {
            issues.Add("Bitte wählen Sie ein AIS-Profil aus.");
        }

        if (string.IsNullOrWhiteSpace(deviceProfileId))
        {
            issues.Add("Bitte wählen Sie ein Geräteprofil aus.");
        }

        if (string.IsNullOrWhiteSpace(exportProfileId))
        {
            issues.Add("Bitte wählen Sie ein Exportprofil aus.");
        }

        return issues;
    }

    private static string CreateProfileOriginLabel(ProfileMetadata metadata)
    {
        return metadata.IsBuiltIn ? "BuiltIn" : "UserDefined";
    }

    private sealed record ProfileSelectionItem(string Id, string DisplayName);
}
