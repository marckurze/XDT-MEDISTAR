using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class SaveFeedbackDisplayServiceTests
{
    private readonly SaveFeedbackDisplayService _service = new();

    [Fact]
    public void CreateForSaveResult_ShouldShowSuccessFeedbackAfterSuccessfulSave()
    {
        var display = _service.CreateForSaveResult(isSuccessful: true);

        Assert.True(display.ShowSuccess);
        Assert.Equal("Gespeichert", display.ButtonText);
        Assert.Equal("Gespeichert", display.StatusText);
        Assert.Equal(TimeSpan.FromMilliseconds(1600), display.VisibleDuration);
    }

    [Fact]
    public void CreateForSaveResult_ShouldNotShowSuccessFeedbackAfterFailedSave()
    {
        var display = _service.CreateForSaveResult(isSuccessful: false);

        Assert.False(display.ShowSuccess);
        Assert.Equal("Speichern", display.ButtonText);
        Assert.Equal(string.Empty, display.StatusText);
        Assert.Equal(TimeSpan.Zero, display.VisibleDuration);
    }
}
