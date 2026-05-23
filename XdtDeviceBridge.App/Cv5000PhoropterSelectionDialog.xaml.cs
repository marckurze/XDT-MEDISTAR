using System.Windows;
using System.Windows.Controls;
using XdtDeviceBridge.Core;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfGroupBox = System.Windows.Controls.GroupBox;

namespace XdtDeviceBridge.App;

public partial class Cv5000PhoropterSelectionDialog : Window
{
    private static readonly IReadOnlyList<MeasurementGroupDefinition> GroupDefinitions = new MeasurementGroupDefinition[]
    {
        new MeasurementGroupDefinition("V0", "Lensmeter (V0)"),
        new MeasurementGroupDefinition("V1", "Autorefraktor (V1)"),
        new MeasurementGroupDefinition("V2", "Phoropter (V2)"),
        new MeasurementGroupDefinition("V3", "Brillenrezept (V3)"),
        new MeasurementGroupDefinition("V4", "Autorefraktor subjektiv (V4)"),
        new MeasurementGroupDefinition("V7", "Keratometer (V7)"),
        new MeasurementGroupDefinition("P", "Pachymetrie (P)"),
        new MeasurementGroupDefinition("Y", "Tonometrie (Y)")
    };

    private static readonly HashSet<string> DefaultSelectionPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "V0",
        "V1",
        "V2"
    };

    private readonly List<SelectionRow> _rows = new();

    public Cv5000PhoropterSelectionDialog(MedistarHistoricalMeasurementParseResult parseResult)
    {
        ArgumentNullException.ThrowIfNull(parseResult);

        InitializeComponent();

        SelectedMeasurements = Array.Empty<AisHistoricalMeasurementRecord>();
        PopulatePatientHeader(parseResult.Patient);
        PopulateGroups(parseResult.Records);
        PopulateStatus(parseResult);
        UpdateExportButton();
    }

    public IReadOnlyList<AisHistoricalMeasurementRecord> SelectedMeasurements { get; private set; }

    private void PopulatePatientHeader(PatientData patient)
    {
        PatientNumberTextBlock.Text = DisplayOrDash(patient.PatientNumber);
        ExaminationTypeTextBlock.Text = DisplayOrDash(patient.ExaminationType);
        PatientNameTextBlock.Text = $"{DisplayOrDash(patient.LastName)}, {DisplayOrDash(patient.FirstName)}";
        BirthDateTextBlock.Text = DisplayOrDash(patient.BirthDate);
    }

    private void PopulateGroups(IReadOnlyList<AisHistoricalMeasurementRecord> records)
    {
        GroupsPanel.Children.Clear();
        _rows.Clear();

        foreach (var groupDefinition in GroupDefinitions)
        {
            var groupRecords = records
                .Where(record => string.Equals(record.SourcePrefix, groupDefinition.Prefix, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(record => record.Date)
                .ThenBy(record => record.Variant, StringComparer.OrdinalIgnoreCase)
                .ThenBy(record => string.Join(" ", record.OriginalLines), StringComparer.OrdinalIgnoreCase)
                .ToList();

            var groupBox = new WpfGroupBox
            {
                Header = groupDefinition.Header,
                Margin = new Thickness(0, 0, 0, 10)
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10, 6, 10, 8) };
            groupBox.Content = stackPanel;

            if (groupRecords.Count == 0)
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = "Keine Daten vorhanden",
                    Foreground = System.Windows.SystemColors.GrayTextBrush
                });
                GroupsPanel.Children.Add(groupBox);
                continue;
            }

            var newestExportable = groupRecords.FirstOrDefault(record => record.IsExportableToCv5000);
            foreach (var record in groupRecords)
            {
                var isDefaultSelected = record.IsExportableToCv5000
                    && ReferenceEquals(record, newestExportable)
                    && DefaultSelectionPrefixes.Contains(record.SourcePrefix);
                var checkBox = new WpfCheckBox
                {
                    IsEnabled = record.IsExportableToCv5000,
                    IsChecked = isDefaultSelected,
                    Margin = new Thickness(0, 2, 0, 2),
                    Content = CreateDisplayText(record),
                    ToolTip = string.Join(Environment.NewLine, record.OriginalLines)
                };
                checkBox.Checked += SelectionCheckBox_Changed;
                checkBox.Unchecked += SelectionCheckBox_Changed;
                stackPanel.Children.Add(checkBox);
                _rows.Add(new SelectionRow(record, checkBox));

                if (!record.IsExportableToCv5000)
                {
                    stackPanel.Children.Add(new TextBlock
                    {
                        Margin = new Thickness(24, 0, 0, 4),
                        Foreground = System.Windows.SystemColors.GrayTextBrush,
                        Text = CreateDisabledReason(record)
                    });
                }
            }

            GroupsPanel.Children.Add(groupBox);
        }
    }

    private void PopulateStatus(MedistarHistoricalMeasurementParseResult parseResult)
    {
        var messages = new List<string>();
        if (!_rows.Any(row => row.Record.IsExportableToCv5000))
        {
            messages.Add("Keine an CV-5000 übergebbaren refraktiven Werte gefunden.");
        }

        if (parseResult.Warnings.Count > 0)
        {
            messages.Add($"Hinweise aus der AIS-Datei: {string.Join(" ", parseResult.Warnings)}");
        }

        StatusTextBlock.Text = string.Join(Environment.NewLine, messages);
    }

    private static string CreateDisplayText(AisHistoricalMeasurementRecord record)
    {
        var parts = new List<string>
        {
            record.Date.ToString("dd.MM.yyyy"),
            record.SourcePrefix
        };
        if (!string.IsNullOrWhiteSpace(record.Variant))
        {
            parts.Add(record.Variant);
        }

        AddEyeDisplay(parts, "R", record.RightEye);
        AddEyeDisplay(parts, "L", record.LeftEye);
        if (!string.IsNullOrWhiteSpace(record.Pd))
        {
            parts.Add($"PD={record.Pd}");
        }

        if (!string.IsNullOrWhiteSpace(record.Vd))
        {
            parts.Add($"VD={record.Vd}");
        }

        return string.Join("  ", parts);
    }

    private static void AddEyeDisplay(List<string> parts, string eye, AisHistoricalEyeRefraction? values)
    {
        if (values is null)
        {
            return;
        }

        var text = $"{eye}: S={DisplayOrDash(values.Sphere)}";
        if (!string.IsNullOrWhiteSpace(values.Cylinder) || !string.IsNullOrWhiteSpace(values.Axis))
        {
            text += $" Z={DisplayOrDash(values.Cylinder)}*{DisplayOrDash(values.Axis)}";
        }

        if (!string.IsNullOrWhiteSpace(values.Add))
        {
            text += $" A={values.Add}";
        }

        parts.Add(text);
    }

    private static string CreateDisabledReason(AisHistoricalMeasurementRecord record)
    {
        if (record.SourceKind is AisHistoricalMeasurementSourceKind.Keratometry
            or AisHistoricalMeasurementSourceKind.Pachymetry
            or AisHistoricalMeasurementSourceKind.Tonometry)
        {
            return "Für CV-5000-Import noch nicht freigegeben.";
        }

        return record.ParseWarnings.Count == 0
            ? "Nicht exportierbar: keine vollständige R-/L-Refraktion gefunden."
            : string.Join(" ", record.ParseWarnings);
    }

    private void SelectionCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        UpdateExportButton();
    }

    private void UpdateExportButton()
    {
        ExportButton.IsEnabled = _rows.Any(row => row.CheckBox.IsEnabled && row.CheckBox.IsChecked == true);
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        var selected = _rows
            .Where(row => row.CheckBox.IsEnabled && row.CheckBox.IsChecked == true)
            .Select(row => row.Record)
            .ToArray();

        if (selected.Length == 0)
        {
            StatusTextBlock.Text = "Bitte mindestens einen exportierbaren refraktiven Datensatz auswählen.";
            UpdateExportButton();
            return;
        }

        SelectedMeasurements = selected;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private static string DisplayOrDash(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    }

    private sealed record MeasurementGroupDefinition(string Prefix, string Header);

    private sealed record SelectionRow(AisHistoricalMeasurementRecord Record, WpfCheckBox CheckBox);
}
