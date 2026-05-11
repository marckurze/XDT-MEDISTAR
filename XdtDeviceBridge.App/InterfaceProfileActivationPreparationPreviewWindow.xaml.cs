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
        SetSectionVisibility(WarningConfirmationItemsSection, preview.WarningConfirmationItems.Count > 0);
        SetSectionVisibility(
            ActivationPlanMissingRequirementsSection,
            preview.ActivationPlanMissingRequirements.Count > 0);
        SetSectionVisibility(
            ActivationPlanReasonsSection,
            preview.ActivationPlanReasons.Count > 0
            && preview.WarningConfirmationItems.Count == 0
            && preview.ActivationPlanMissingRequirements.Count == 0);
        SetSectionVisibility(ActivationPlanStepsSection, preview.ActivationPlanSteps.Count > 0);
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
