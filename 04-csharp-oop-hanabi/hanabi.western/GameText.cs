using Godot;

internal static class GameText
{
    public static bool English => TranslationServer.GetLocale().StartsWith("en", StringComparison.OrdinalIgnoreCase);

    public static void Use(string locale) => TranslationServer.SetLocale(locale);

    public static string Pick(string russian, string english) => English ? english : russian;

    public static string Path(string fileName) => English ? $"res://texts/en/{fileName}" : $"res://texts/{fileName}";
}
