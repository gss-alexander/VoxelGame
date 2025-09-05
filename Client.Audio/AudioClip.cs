using OpenAL;

namespace Client.Audio;

public class AudioClip : IDisposable
{
    public int Buffer => (int)_bufferHandle;
    
    public int Format { get; }
    public int Frequency { get; }
    public byte[] Data { get; }

    private readonly uint _bufferHandle;

    public AudioClip(byte[] data, int format, int frequency)
    {
        Data = data;
        Format = format;
        Frequency = frequency;
        
        Al.GenBuffer(out _bufferHandle);
        UploadBufferData();
    }
    
    private unsafe void UploadBufferData()
    {
        fixed (void* data = Data)
        {
            Al.BufferData(_bufferHandle, Format, data, Data.Length, Frequency);
            CheckAlError("BufferData");
        }
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
        Al.DeleteBuffer(_bufferHandle);
    }
}