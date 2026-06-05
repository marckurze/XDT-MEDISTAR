using System.Windows;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.App;

public partial class AisOutputInfoWindow : Window
{
    public AisOutputInfoWindow(AisOutputInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        InitializeComponent();
        DataContext = new AisOutputInfoViewModel(info);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

public sealed class AisOutputInfoViewModel
{
    public AisOutputInfoViewModel(AisOutputInfo info)
    {
        InterfaceProfileName = info.InterfaceProfileName;
        AisProfileName = info.AisProfileName;
        DeviceProfileName = info.DeviceProfileName;
        ExportProfileName = info.ExportProfileName;
        DeviceSummary = $"{info.Manufacturer} / {info.DeviceType}";
        ConnectionKind = info.ConnectionKind;
        ActiveText = info.IsActive ? "Aktiv" : "Inaktiv";
        DefaultExaminationType = info.DefaultExaminationType;
        ExaminationTypeHint = info.ExaminationTypeHint;
        Fields = info.Fields;
    }

    public string InterfaceProfileName { get; }

    public string AisProfileName { get; }

    public string DeviceProfileName { get; }

    public string ExportProfileName { get; }

    public string DeviceSummary { get; }

    public string ConnectionKind { get; }

    public string ActiveText { get; }

    public string DefaultExaminationType { get; }

    public string ExaminationTypeHint { get; }

    public IReadOnlyList<AisOutputFieldInfo> Fields { get; }
}
