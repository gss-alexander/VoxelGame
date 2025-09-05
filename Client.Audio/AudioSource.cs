using OpenAL;

namespace Client.Audio;

public class AudioSource : IDisposable
{
    public bool IsPlaying
    {
        get
        {
            Al.GetSourcei(_sourceHandle, Al.SourceState, out var state);
            return state == Al.Playing;
        }
    }

    public AudioClip? Clip
    {
        get => _clip;
        set
        {
            _clip = value;
            BindClipBuffer();
        }
    }

    private AudioClip? _clip = null;

    private readonly uint _sourceHandle;

    public AudioSource()
    {
        Al.GenSource(out _sourceHandle);
        CheckAlError("GenSource");
    }
    
    public void Play(bool isLooping = false)
    {
        if (Clip == null)
        {
            Console.WriteLine($"[Audio Source]: Tried playing with a null clip. Ignoring");
            return;
        }
        
        Al.Sourcei(_sourceHandle, Al.Looping, isLooping ? Al.True : Al.False);
        Al.SourcePlay(_sourceHandle);
    }

    public void Stop()
    {
        Al.SourceStop(_sourceHandle);
        Al.Sourcei(_sourceHandle, Al.Looping, Al.False);
    }

    private void BindClipBuffer()
    {
        if (Clip == null)
        {
            return;
        }
        
        Al.Sourcei(_sourceHandle, Al.Buffer, Clip.Buffer);
        CheckAlError("Sourcei");
    }

    private static void CheckAlError(string operation)
    {
        var error = Al.GetError();
        if (error != Al.NoError)
        {
            throw new Exception($"[Audio Source]: OpenAL error in {operation}: {error}");
        }
    }

    public void Dispose()
    {
        Al.DeleteSource(_sourceHandle);
        if (Clip != null)
        {
            Clip.Dispose();
        }
    }
}