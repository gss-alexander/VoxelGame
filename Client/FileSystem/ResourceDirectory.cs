namespace Client.FileSystem;

public static class ResourceDirectory
{
    // Executable is placed in Root/bin/(Env)/net8.0/executable
    // Resource directory is placed in Root/Resources/*
    public static readonly string DirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Resources");
}