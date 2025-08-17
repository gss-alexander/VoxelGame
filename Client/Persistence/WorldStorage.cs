namespace Client.Persistence;

public class WorldStorage
{
    private static readonly string StoragePath =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppConstants.GameTitle,
            "Worlds"
        );

    public static void Initialize()
    {
        Console.WriteLine("Initializing world storage");

        if (!Directory.Exists(StoragePath))
        {
            Directory.CreateDirectory(StoragePath);
            Console.WriteLine($"Created new storage directory at: {StoragePath}");
        }
    }

    public static void StoreWorld(WorldData data)
    {
        var tempFilePath = Path.GetTempFileName();
        using var fileStream = new FileStream(tempFilePath, FileMode.Open);
        using var fileWriter = new StreamWriter(fileStream);

        Console.WriteLine($"Writing to temp file: {tempFilePath}");
        var serializedWorldData = data.Serialize();
        fileWriter.Write(serializedWorldData);
        fileWriter.Flush();

        var tempFileName = Path.GetFileName(tempFilePath);
        var copiedFilePath = Path.Combine(StoragePath, tempFileName);
        Console.WriteLine($"Copying temp file from \"{tempFilePath}\" to \"{copiedFilePath}\"");
        File.Copy(tempFilePath, copiedFilePath);

        if (DoesWorldExist(data.Name))
        {
            Console.WriteLine($"Existing save file found. Deleting...");
            File.Delete(GetWorldFilePath(data.Name));
        }
        
        Console.WriteLine($"Writing to: {GetWorldFilePath(data.Name)}");
        File.Copy(copiedFilePath, GetWorldFilePath(data.Name));
        
        Console.WriteLine($"Deleting copied file");
        File.Delete(copiedFilePath);
    }

    public static WorldData LoadFromName(string worldName)
    {
        if (!DoesWorldExist(worldName))
        {
            throw new InvalidOperationException($"Could not find file for world {worldName}");
        }

        var filePath = GetWorldFilePath(worldName);
        var contents = File.ReadAllText(filePath);
        return WorldData.Deserialize(contents);
    }

    public static bool DoesWorldExist(string worldName)
    {
        return File.Exists(GetWorldFilePath(worldName));
    }

    private static string GetWorldFilePath(string worldName)
    {
        return Path.Combine(StoragePath, $"{worldName}.world");
    }
}