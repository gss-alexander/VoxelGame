using OpenAL;

namespace Client.Audio;

public class AudioContext : IDisposable
{
    private readonly IntPtr _device;
    private readonly IntPtr _context;

    public AudioContext()
    {
        _device = Alc.OpenDevice(null);
        if (_device == IntPtr.Zero)
        {
            throw new Exception("Failed to open default audio device");
        }
        
        _context = Alc.CreateContext(_device, null);
        if (_context == IntPtr.Zero)
        {
            Alc.CloseDevice(_device);
            throw new Exception("Failed to create audio context");
        }

        if (!Alc.MakeContextCurrent(_context))
        {
            throw new Exception("Failed to make context current");
        }
    }

    public void Dispose()
    {
        if (_context != IntPtr.Zero)
        {
            Alc.MakeContextCurrent(IntPtr.Zero);
            Alc.DestroyContext(_context);
        }

        if (_device != IntPtr.Zero)
        {
            Alc.CloseDevice(_device);
        }
    }
}