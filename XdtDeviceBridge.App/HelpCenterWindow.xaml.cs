using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace XdtDeviceBridge.App;

public partial class HelpCenterWindow : Window
{
    private const string HelpResourcePath = "Assets/Help/xdtbox-help.md";

    public HelpCenterWindow()
    {
        InitializeComponent();
        var topics = LoadTopics();
        HelpTopicsListBox.ItemsSource = topics;
        if (topics.Count > 0)
        {
            HelpTopicsListBox.SelectedIndex = 0;
        }
    }

    private void HelpTopicsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (HelpTopicsListBox.SelectedItem is HelpTopic topic)
        {
            HelpContentTextBox.Text = topic.Content;
        }
    }

    private static IReadOnlyList<HelpTopic> LoadTopics()
    {
        var streamInfo = System.Windows.Application.GetResourceStream(new Uri(HelpResourcePath, UriKind.Relative));
        if (streamInfo is null)
        {
            return new[] { new HelpTopic("Hilfe nicht gefunden", "Die lokale Hilfedatei konnte nicht geladen werden.") };
        }

        using var reader = new StreamReader(streamInfo.Stream);
        return ParseTopics(reader.ReadToEnd());
    }

    private static IReadOnlyList<HelpTopic> ParseTopics(string markdown)
    {
        var topics = new List<HelpTopic>();
        string? currentTitle = null;
        var builder = new StringBuilder();

        foreach (var rawLine in markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            if (rawLine.StartsWith("# ", StringComparison.Ordinal))
            {
                AddCurrentTopic();
                currentTitle = rawLine[2..].Trim();
                builder.Clear();
                continue;
            }

            builder.AppendLine(rawLine);
        }

        AddCurrentTopic();
        return topics;

        void AddCurrentTopic()
        {
            if (string.IsNullOrWhiteSpace(currentTitle))
            {
                return;
            }

            topics.Add(new HelpTopic(currentTitle, builder.ToString().Trim()));
        }
    }

    private sealed record HelpTopic(string Title, string Content);
}
