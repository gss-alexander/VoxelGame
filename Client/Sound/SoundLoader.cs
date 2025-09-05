using Client.Audio;
using Client.Blocks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Client.Sound;

public class SoundLoader
{
    private class SoundConfig
    {
        public Dictionary<string, SoundData> Sounds { get; set; } = new();
    }

    private class SoundData
    {
        public string Path { get; set; }
    }
    
    public static Dictionary<string, AudioClip> LoadAudio(string dataFilePath)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        var fileContent = File.ReadAllText(dataFilePath);
        var config = deserializer.Deserialize<SoundConfig>(fileContent);

        var clips = new Dictionary<string, AudioClip>();
        foreach (var kvp in config.Sounds)
        {
            var clipPath = Path.Combine(dataFilePath, "..", "..", "Audio", kvp.Value.Path);
            var clipData = AudioFileLoader.LoadWav(clipPath);
            clips.Add(kvp.Key, clipData);
        }

        Console.WriteLine($"Loaded {clips.Count} sound clips from data");
        return clips;
    }
}