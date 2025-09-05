using Client.Audio;

namespace Client.Sound;

public class SoundPlayer
{
    private readonly Dictionary<string, AudioClip> _audioClips;
    private readonly AudioContext _audioContext;
    private readonly AudioSource _audioSource;

    private string _lastSoundId;

    public SoundPlayer()
    {
        _audioContext = new AudioContext();
        _audioSource = new AudioSource();

        _audioClips = SoundLoader.LoadAudio(Path.Combine("..", "..", "..", "Resources", "Data", "sounds.yaml"));
    }

    public void PlaySound(string soundId)
    {
        if (_lastSoundId != soundId)
        {
            if (!_audioClips.TryGetValue(soundId, out var clip))
            {
                Console.WriteLine($"[Sound Player]: Could not find sound with id: {soundId}");
                return;
            }

            _audioSource.Clip = clip;
        }
        
        Console.WriteLine($"[Sound Player]: Playing sound: {soundId}");
        _audioSource.Play(false);
        _lastSoundId = soundId;
    }

    public void Stop()
    {
        _audioSource.Stop();
    }
}