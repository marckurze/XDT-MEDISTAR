namespace XdtDeviceBridge.Infrastructure;

public sealed record SaveFeedbackDisplay(
    bool ShowSuccess,
    string ButtonText,
    string StatusText,
    TimeSpan VisibleDuration);

public sealed class SaveFeedbackDisplayService
{
    private static readonly TimeSpan SuccessDuration = TimeSpan.FromMilliseconds(1600);

    public SaveFeedbackDisplay CreateForSaveResult(bool isSuccessful)
    {
        return isSuccessful
            ? new SaveFeedbackDisplay(
                ShowSuccess: true,
                ButtonText: "Gespeichert",
                StatusText: "Gespeichert",
                VisibleDuration: SuccessDuration)
            : new SaveFeedbackDisplay(
                ShowSuccess: false,
                ButtonText: "Speichern",
                StatusText: string.Empty,
                VisibleDuration: TimeSpan.Zero);
    }
}
