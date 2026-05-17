using System.Media;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.App;

public sealed class WavInterfaceProfileNotificationSoundPlayer : IInterfaceProfileNotificationSoundPlayer
{
    private SoundPlayer? _player;

    public void Play(string soundFilePath)
    {
        _player?.Stop();
        _player?.Dispose();
        _player = new SoundPlayer(soundFilePath);
        _player.Play();
    }
}
