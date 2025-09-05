using Client.Audio;

namespace Client.Sound;

public class SoundPlayer
{
    public int ActiveSoundSources => _activeSources.Count;
    
    private readonly Dictionary<string, AudioClip> _audioClips;
    private readonly AudioContext _audioContext;
    private readonly AudioSource _audioSource;

    private readonly ObjectPool<AudioSource> _audioSourcePool;
    private readonly List<AudioSource> _activeSources = new();

    private string _lastSoundId;

    public SoundPlayer()
    {
        _audioContext = new AudioContext();
        _audioSource = new AudioSource();

        _audioClips = SoundLoader.LoadAudio(Path.Combine("..", "..", "..", "Resources", "Data", "sounds.yaml"));

        _audioSourcePool = new ObjectPool<AudioSource>(() =>
        {
            return new AudioSource();
        }, source =>
        {
            source.Clip = null;
        });
    }

    public void Update()
    {
        var sourcesToRemove = new List<AudioSource>();
        foreach (var source in _activeSources)
        {
            if (!source.IsPlaying)
            {
                sourcesToRemove.Add(source);
            }
        }

        foreach (var sourceToRemove in sourcesToRemove)
        {
            _activeSources.Remove(sourceToRemove);
            _audioSourcePool.Release(sourceToRemove);
        }
    }

    public void PlaySound(string soundId)
    {
        var source = _audioSourcePool.Get();
        if (!_audioClips.TryGetValue(soundId, out var clip))
        {
            Console.WriteLine($"[Sound Player]: Could not find sound with id: {soundId}");
            return;
        }
        source.Clip = clip;
        
        
        Console.WriteLine($"[Sound Player]: Playing sound: {soundId}");
        source.Play(false);
        
        _activeSources.Add(source);
    }

    public void Stop()
    {
        _audioSource.Stop();
    }
}