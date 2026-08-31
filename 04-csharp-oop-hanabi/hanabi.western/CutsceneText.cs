using Godot;

public partial class CutsceneText : Label
{
    private const float CharactersPerSecond = 22f;
    private float visibleChars;
    private string fullText = "";

    public string TextPath { get; init; } = "";

    public override void _Ready()
    {
        Position = new Vector2(92, 38);
        Size = new Vector2(968, 148);
        AutowrapMode = TextServer.AutowrapMode.WordSmart;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
        MouseFilter = MouseFilterEnum.Ignore;
        ZIndex = 100;
        AddThemeColorOverride("font_color", new Color("f5e5b8"));
        AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.84f));
        AddThemeConstantOverride("outline_size", 5);
        AddThemeFontSizeOverride("font_size", 22);
        fullText = ReadMarkdownText(TextPath);
        Text = "";
    }

    public override void _Process(double delta)
    {
        if (visibleChars >= fullText.Length)
        {
            return;
        }

        visibleChars = Mathf.Min(fullText.Length, visibleChars + (float)delta * CharactersPerSecond);
        Text = fullText[..(int)visibleChars];
    }

    private static string ReadMarkdownText(string path)
    {
        var lines = new List<string>();
        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        if (file is null)
        {
            return "";
        }

        while (!file.EofReached())
        {
            var line = file.GetLine().Trim();
            if (line.Length > 0 && !line.StartsWith("<!--") && !line.StartsWith("#"))
            {
                lines.Add(line);
            }
        }

        return string.Join("\n\n", lines);
    }
}
