using System.Windows;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.App;

public partial class InterfaceProfileActivationPreparationPreviewWindow : Window
{
    public InterfaceProfileActivationPreparationPreviewWindow(
        InterfaceProfileActivationPreparationPreview preview)
    {
        InitializeComponent();
        DataContext = preview;

        SetSectionVisibility(GuardMessageTextBlock, !string.IsNullOrWhiteSpace(preview.GuardMessage));
        SetSectionVisibility(BlockersSection, preview.ImportantBlockers.Count > 0);
        SetSectionVisibility(WarningsSection, preview.ImportantWarnings.Count > 0);
        SetSectionVisibility(InfosSection, preview.ImportantInfos.Count > 0);
    }

    private static void SetSectionVisibility(FrameworkElement section, bool isVisible)
    {
        section.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
