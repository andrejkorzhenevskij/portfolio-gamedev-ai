using Godot;

public partial class MistakeOneCutscene : Control
{
    private const string BackgroundPath = "res://assets/mistake_one_prairie.png";
    private const string RidersPath = "res://assets/mistake_one_riders.png";
    private const float Duration = 4.15f;
    private const float FadeInDuration = 1.2f;
    private const int FrameWidth = 543;
    private const int FrameHeight = 724;

    private TextureRect riders = null!;
    private Texture2D sheet = null!;
    private float elapsed;
    private int frame = -1;

    public Action? Finished { get; init; }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        ZIndex = 1000;
        Modulate = new Color(1, 1, 1, 0);

        AddChild(new TextureRect
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            Texture = GD.Load<Texture2D>(BackgroundPath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            MouseFilter = MouseFilterEnum.Ignore
        });

        sheet = GD.Load<Texture2D>(RidersPath);
        riders = new TextureRect
        {
            Position = new Vector2(400, 258),
            Size = new Vector2(152, 203),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(riders);
        AddChild(new CutsceneText { TextPath = GameText.Path("mistake-1.md") });
        SetFrame(0);
    }

    public override void _Process(double delta)
    {
        elapsed += (float)delta * 0.4f;
        var alpha = elapsed < FadeInDuration
            ? elapsed / FadeInDuration
            : elapsed > 3.45f ? (Duration - elapsed) / (Duration - 3.45f) : 1f;
        Modulate = new Color(1, 1, 1, Mathf.Clamp(alpha, 0f, 1f));

        var travel = Mathf.SmoothStep(0f, 1f, Mathf.Min(elapsed / 2.25f, 1f));
        riders.Position = new Vector2(Mathf.Lerp(400f, 550f, travel), 258f);
        SetFrame(elapsed switch
        {
            < 1.15f => 0,
            < 1.60f => 1,
            < 2.10f => 2,
            _ => 3
        });

        if (elapsed < Duration)
        {
            return;
        }

        Finished?.Invoke();
        QueueFree();
    }

    public override void _Input(InputEvent @event)
    {
        GetViewport().SetInputAsHandled();
    }

    private void SetFrame(int nextFrame)
    {
        if (frame == nextFrame)
        {
            return;
        }

        frame = nextFrame;
        riders.Texture = new AtlasTexture
        {
            Atlas = sheet,
            Region = new Rect2(frame * FrameWidth, 0, FrameWidth, FrameHeight),
            FilterClip = true
        };
    }
}
