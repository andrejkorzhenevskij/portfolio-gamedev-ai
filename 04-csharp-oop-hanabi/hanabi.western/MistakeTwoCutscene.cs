using Godot;

public partial class MistakeTwoCutscene : Control
{
    private const string BackgroundPath = "res://assets/mistake_two_canyon.png";
    private const string ActorsPath = "res://assets/mistake_two_actors.png";
    private const float Duration = 8.8f;
    private const float FadeInDuration = 1.2f;
    private const float TelescopeStart = 2.35f;
    private const int FrameSize = 887;

    private readonly List<(TextureRect Node, CpuParticles2D Dust, float Delay, float StartX, float Y, float Speed)> actors = [];
    private float elapsed;
    private ColorRect telescopeMask = null!;
    private ShaderMaterial telescopeMaterial = null!;

    public Action? Finished { get; init; }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        ZIndex = 1000;
        Modulate = new Color(1, 1, 1, 0);

        var background = GD.Load<Texture2D>(BackgroundPath);
        AddChild(new TextureRect
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            Texture = background,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            MouseFilter = MouseFilterEnum.Ignore
        });

        var sheet = GD.Load<Texture2D>(ActorsPath);
        var dustTexture = CreateDustTexture();
        AddActor(sheet, dustTexture, frame: 0, size: 230, delay: 0.55f, startX: 125, y: 229, speed: 88);
        AddActor(sheet, dustTexture, frame: 1, size: 105, delay: 0.55f, startX: -50, y: 310, speed: 65);
        AddActor(sheet, dustTexture, frame: 1, size: 105, delay: 0.55f, startX: 55, y: 310, speed: 62);
        AddActor(sheet, dustTexture, frame: 1, size: 105, delay: 0.55f, startX: 160, y: 310, speed: 59);
        AddActor(sheet, dustTexture, frame: 1, size: 105, delay: 0.55f, startX: 265, y: 310, speed: 56);

        AddChild(new TextureRect
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            Texture = background,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            MouseFilter = MouseFilterEnum.Ignore,
            ZIndex = 10,
            Material = new ShaderMaterial
            {
                Shader = new Shader
                {
                    Code = """
                        shader_type canvas_item;

                        void fragment() {
                            vec4 color = texture(TEXTURE, UV);
                            float edge = mix(0.235, 0.175, smoothstep(0.0, 0.72, UV.y));
                            color.a *= 1.0 - smoothstep(edge - 0.008, edge + 0.006, UV.x);
                            COLOR = color;
                        }
                        """
                }
            }
        });

        telescopeMaterial = new ShaderMaterial
        {
            Shader = new Shader
            {
                Code = """
                    shader_type canvas_item;
                    uniform vec2 center = vec2(0.31, 0.58);

                    void fragment() {
                        vec2 p = (UV - center) * vec2(1.777778, 1.0);
                        float shade = smoothstep(0.435, 0.455, length(p));
                        COLOR = vec4(0.004, 0.002, 0.001, shade);
                    }
                    """
            }
        };
        telescopeMask = new ColorRect
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            Color = Colors.White,
            MouseFilter = MouseFilterEnum.Ignore,
            Visible = false,
            ZIndex = 20,
            Material = telescopeMaterial
        };
        AddChild(telescopeMask);
        AddChild(new CutsceneText { TextPath = GameText.Path("mistake-2.md") });
    }

    public override void _Process(double delta)
    {
        elapsed += (float)delta * 0.75f;
        var alpha = elapsed < FadeInDuration
            ? elapsed / FadeInDuration
            : elapsed > 8.05f ? (Duration - elapsed) / (Duration - 8.05f) : 1f;
        Modulate = new Color(1, 1, 1, Mathf.Clamp(alpha, 0f, 1f));
        var throughTelescope = elapsed >= TelescopeStart;
        telescopeMask.Visible = throughTelescope;
        var telescopePan = Mathf.SmoothStep(0, 1, Mathf.Clamp((elapsed - TelescopeStart) / (Duration - TelescopeStart), 0, 1));
        telescopeMaterial.SetShaderParameter("center", new Vector2(Mathf.Lerp(0.31f, 0.66f, telescopePan), 0.58f));

        for (var index = 0; index < actors.Count; index++)
        {
            var actor = actors[index];
            var travelTime = elapsed - actor.Delay;
            actor.Node.Visible = travelTime >= 0 && (index == 0 || throughTelescope);
            actor.Dust.Emitting = actor.Node.Visible;
            if (!actor.Node.Visible)
            {
                continue;
            }

            var bob = index == 0 ? 0 : Mathf.Sin(travelTime * 6f + index * 0.7f) * (throughTelescope ? 2f : 4f);
            actor.Node.Scale = throughTelescope ? Vector2.One * 1.15f : Vector2.One;
            actor.Node.Position = new Vector2(actor.StartX + travelTime * actor.Speed, actor.Y + bob);
        }

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

    private void AddActor(Texture2D sheet, Texture2D dustTexture, int frame, float size, float delay, float startX, float y, float speed)
    {
        var cropLeft = frame == 1 ? 64 : 0;
        var actor = new TextureRect
        {
            Size = new Vector2(size, size),
            Texture = new AtlasTexture
            {
                Atlas = sheet,
                Region = new Rect2(frame * FrameSize + cropLeft, 0, FrameSize - cropLeft, FrameSize),
                FilterClip = true
            },
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore,
            Visible = false
        };
        var dust = new CpuParticles2D
        {
            Position = new Vector2(size * 0.5f, size * 0.69f),
            Amount = frame == 0 ? 64 : 42,
            Lifetime = 1.25,
            Randomness = 0.65f,
            LocalCoords = false,
            EmissionShape = CpuParticles2D.EmissionShapeEnum.Rectangle,
            EmissionRectExtents = new Vector2(size * 0.4f, 3),
            Direction = new Vector2(-1, -0.22f),
            Spread = 32,
            Gravity = new Vector2(0, -4),
            InitialVelocityMin = 8,
            InitialVelocityMax = 24,
            ScaleAmountMin = 0.8f,
            ScaleAmountMax = frame == 0 ? 2.1f : 1.7f,
            Texture = dustTexture,
            ColorRamp = new Gradient
            {
                Offsets = [0, 0.6f, 1],
                Colors = [new Color("f2c471d0"), new Color("d69a4590"), new Color("8c531800")]
            },
            Emitting = false,
            ShowBehindParent = true
        };
        actor.AddChild(new Sprite2D
        {
            Texture = dustTexture,
            Position = new Vector2(size * 0.5f, size * 0.695f),
            Scale = new Vector2(frame == 0 ? 7.5f : 5.6f, 0.58f),
            Modulate = new Color(0.12f, 0.06f, 0.015f, 0.32f),
            ShowBehindParent = true
        });
        actor.AddChild(dust);
        actors.Add((actor, dust, delay, startX, y, speed));
        AddChild(actor);
    }

    private static Texture2D CreateDustTexture() => new GradientTexture2D
    {
        Width = 24,
        Height = 24,
        Fill = GradientTexture2D.FillEnum.Radial,
        FillFrom = new Vector2(0.5f, 0.5f),
        FillTo = new Vector2(0.5f, 0),
        Gradient = new Gradient
        {
            Offsets = [0, 0.55f, 1],
            Colors = [Colors.White, new Color(1, 1, 1, 0.55f), Colors.Transparent]
        }
    };
}
