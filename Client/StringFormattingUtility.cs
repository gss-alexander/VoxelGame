namespace Client;

public static class StringFormattingUtility
{
    public static string ToSnakeCase(this string source)
    {
        return source.Replace(" ", "_");
    }
}