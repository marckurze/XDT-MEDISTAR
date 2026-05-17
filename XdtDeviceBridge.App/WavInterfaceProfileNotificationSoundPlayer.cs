using System.Media;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.App;

public sealed class WavInterfaceProfileNotificationSoundPlayer : IInterfaceProfileNotificationSoundPlayer
{
    private SoundPlayer? _player;
    private string? _currentSoundFilePath;

    public void Play(string soundFilePath)
    {
        if (_player is null
            || !string.Equals(_currentSoundFilePath, soundFilePath, StringComparison.OrdinalIgnoreCase))
        {
            _player?.Dispose();
            _player = new SoundPlayer(soundFilePath);
            _currentSoundFilePath = soundFilePath;
        }

        _player.Play();
    }
}
