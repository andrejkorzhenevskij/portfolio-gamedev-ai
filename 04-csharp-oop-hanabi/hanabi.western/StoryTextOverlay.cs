using Godot;

public readonly record struct StoryTextSegment(bool Black, string[] Lines, bool RevealAfterFirstLine = false);

public partial class StoryTextOverlay : Control
{
    private const float CharactersPerSecond = 22f;
    private static readonly Vector2 TextPosition = new(96, 62);
    private static readonly Vector2 TextSize = new(960, 560);

    private readonly ColorRect black = new();
    private readonly Panel caption = new();
    private readonly Label label = new();
    private readonly List<string> printedLines = [];
    private int segmentIndex;
    private int lineIndex;
    private float visibleChars;
    private bool waitingForClick;
    private bool finishing;
    private bool completed;

    public StoryTextSegment[] Segments { get; init; } = [];
    public Action? Finished { get; init; }
    public bool Active => !completed;
    public bool ShowingBlack => black.Visible;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        ZIndex = 900;

        black.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        black.Color = Colors.Black;
        black.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(black);

        caption.Position = TextPosition - new Vector2(18, 18);
        caption.Size = TextSize + new Vector2(36, 36);
        caption.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.03f, 0.02f, 0.01f, 0.72f),
            BorderColor = new Color("d6c07a"),
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6
        });
        AddChild(caption);

        label.Position = TextPosition;
        label.Size = TextSize;
        label.HorizontalAlignment = HorizontalAlignment.Left;
        label.VerticalAlignment = VerticalAlignment.Top;
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        label.AddThemeColorOverride("font_color", new Color("f5e5b8"));
        label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.82f));
        label.AddThemeConstantOverride("outline_size", 4);
        label.AddThemeFontSizeOverride("font_size", 23);
        AddChild(label);

        StartSegment(0);
    }

    public override void _Process(double delta)
    {
        if (waitingForClick || finishing || Segments.Length == 0)
        {
            return;
        }

        var line = Segments[segmentIndex].Lines[lineIndex];
        visibleChars = Mathf.Min(line.Length, visibleChars + (float)delta * CharactersPerSecond);
        label.Text = TextWith(line[..(int)visibleChars]);
        if (visibleChars >= line.Length)
        {
            FinishLine();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (finishing || !Activated(@event))
        {
            return;
        }

        GetViewport().SetInputAsHandled();
        var line = Segments[segmentIndex].Lines[lineIndex];
        if (!waitingForClick && visibleChars < line.Length)
        {
            visibleChars = line.Length;
            label.Text = TextWith(line);
            FinishLine();
            return;
        }

        StartSegment(segmentIndex + 1);
    }

    public static StoryTextSegment[] LoadSegments(string path, string pictureMarker, string fadeMarker)
    {
        var groups = new[] { new List<string>(), new List<string>(), new List<string>() };
        var group = 0;

        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        if (file is null)
        {
            return [];
        }

        while (!file.EofReached())
        {
            var line = file.GetLine().Trim();
            if (line.Length == 0 || line.StartsWith("<!--") || line.StartsWith("# "))
            {
                continue;
            }

            if (line.StartsWith("###"))
            {
                var marker = line.ToLowerInvariant();
                if (marker.IndexOf(pictureMarker, StringComparison.Ordinal) >= 0)
                {
                    group = 1;
                }
                else if (marker.IndexOf(fadeMarker, StringComparison.Ordinal) >= 0)
                {
                    group = 2;
                }
                continue;
            }

            groups[group].Add(line);
        }

        return
        [
            new(true, [.. groups[0], .. groups[1]], RevealAfterFirstLine: true),
            new(true, groups[2].ToArray())
        ];
    }

    private async System.Threading.Tasks.Task HoldThenReveal()
    {
        finishing = true;
        await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
        black.Visible = false;
        caption.Visible = true;
        finishing = false;
        NextLineOrWait();
    }

    private void FinishLine()
    {
        printedLines.Add(Segments[segmentIndex].Lines[lineIndex]);
        if (Segments[segmentIndex].RevealAfterFirstLine && lineIndex == 0 && black.Visible)
        {
            _ = HoldThenReveal();
            return;
        }

        NextLineOrWait();
    }

    private void NextLineOrWait()
    {
        if (lineIndex + 1 >= Segments[segmentIndex].Lines.Length)
        {
            waitingForClick = true;
            return;
        }

        lineIndex++;
        visibleChars = 0;
        label.Text = TextWith("");
    }

    private string TextWith(string currentLine)
    {
        var lines = printedLines.ToArray();
        var text = string.Join("\n\n", lines);
        return string.IsNullOrEmpty(text) ? currentLine : $"{text}\n\n{currentLine}";
    }

    private void StartSegment(int index)
    {
        while (index < Segments.Length && Segments[index].Lines.Length == 0)
        {
            index++;
        }

        if (index >= Segments.Length)
        {
            completed = true;
            Finished?.Invoke();
            QueueFree();
            return;
        }

        segmentIndex = index;
        black.Visible = Segments[index].Black;
        caption.Visible = !Segments[index].Black;
        printedLines.Clear();
        lineIndex = 0;
        waitingForClick = false;
        visibleChars = 0;
        label.Text = "";
    }

    private static bool Activated(InputEvent @event)
    {
        return @event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } ||
            @event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Enter or Key.KpEnter or Key.Space };
    }
}
