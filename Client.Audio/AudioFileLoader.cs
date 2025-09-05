using OpenAL;

namespace Client.Audio;

public static class AudioFileLoader
{
    public static AudioClip LoadWav(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);

        // Read RIFF header
        string riff = new string(reader.ReadChars(4));
        if (riff != "RIFF")
            throw new Exception("Invalid WAV file: missing RIFF header");

        reader.ReadInt32(); // File size
        
        string wave = new string(reader.ReadChars(4));
        if (wave != "WAVE")
            throw new Exception("Invalid WAV file: missing WAVE header");

        // Read fmt chunk
        string fmt = new string(reader.ReadChars(4));
        if (fmt != "fmt ")
            throw new Exception("Invalid WAV file: missing fmt chunk");

        int fmtSize = reader.ReadInt32();
        int audioFormat = reader.ReadInt16();
        var channels = reader.ReadInt16();
        var frequency = reader.ReadInt32();
        reader.ReadInt32(); // Byte rate
        reader.ReadInt16(); // Block align
        var bitsPerSample = reader.ReadInt16();

        // Skip any extra format bytes
        if (fmtSize > 16)
            stream.Seek(fmtSize - 16, SeekOrigin.Current);

        // Find data chunk
        var format = 0;
        while (true)
        {
            string chunkId = new string(reader.ReadChars(4));
            int chunkSize = reader.ReadInt32();
            
            if (chunkId == "data")
            {
                // Read audio data
                byte[] audioData = reader.ReadBytes(chunkSize);
                
                // Determine OpenAL format
                if (channels == 1)
                    format = bitsPerSample == 8 ? Al.FormatMono8 : Al.FormatMono16;
                else if (channels == 2)
                    format = bitsPerSample == 8 ? Al.FormatStereo8 : Al.FormatStereo16;
                else
                    throw new Exception($"Unsupported channel count: {channels}");

                return new AudioClip(audioData, format, frequency);
            }
            else
            {
                // Skip unknown chunk
                stream.Seek(chunkSize, SeekOrigin.Current);
            }
        }
    }
}