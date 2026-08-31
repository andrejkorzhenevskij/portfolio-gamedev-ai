using Godot;

public partial class GoodGameOverCutscene : Control
{
    private const string BackgroundPath = "res://assets/good_gameover_frontier.png";
    private const string RidersPath = "res://assets/good_gameover_riders.png";
    private const float FadeInDuration = 1.2f;
    private const string DustVeilShader = """
        shader_type canvas_item;
        uniform float progress : hint_range(0.0, 1.0) = 0.0;

        float hash(vec2 p) {
            return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
        }

        float noise(vec2 p) {
            vec2 i = floor(p);
            vec2 f = fract(p);
            f = f * f * (3.0 - 2.0 * f);
            return mix(mix(hash(i), hash(i + vec2(1.0, 0.0)), f.x),
                       mix(hash(i + vec2(0.0, 1.0)), hash(i + vec2(1.0)), f.x), f.y);
        }

        void fragment() {
            vec2 center = vec2(0.5, 0.64);
            vec2 point = (UV - center) * vec2(1.0, 0.78);
            float radius = mix(0.015, 1.35, smoothstep(0.0, 1.0, progress));
            float billow = noise(UV * 7.0 + vec2(TIME * 0.04, -TIME * 0.02));
            float cloud = 1.0 - smoothstep(radius * (0.54 + billow * 0.14), radius, length(point));
            float haze = smoothstep(0.70, 1.0, progress);
            vec3 color = mix(vec3(0.72, 0.45, 0.19), vec3(0.91, 0.70, 0.38), billow);
            COLOR = vec4(color, clamp(cloud * 0.94 + haze, 0.0, 1.0));
        }
        """;

    private TextureRect riders = null!;
    private CpuParticles2D hoofDust = null!;
    private ShaderMaterial dustVeil = null!;
    private Label title = null!;
    private Panel resultPanel = null!;
    private float elapsed;

    public int PlayerShare { get; init; } = 50;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        ZIndex = 1000;
        Modulate = new Color(1, 1, 1, 0);

        AddChild(FullRectTexture(BackgroundPath));

        riders = new TextureRect
        {
            Texture = GD.Load<Texture2D>(RidersPath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(riders);

        hoofDust = new CpuParticles2D
        {
            Amount = 110,
            Lifetime = 1.45,
            Randomness = 0.72f,
            LocalCoords = false,
            EmissionShape = CpuParticles2D.EmissionShapeEnum.Rectangle,
            Direction = new Vector2(0, 1),
            Spread = 65,
            Gravity = new Vector2(0, -3),
            InitialVelocityMin = 10,
            InitialVelocityMax = 34,
            ScaleAmountMin = 0.8f,
            ScaleAmountMax = 2.4f,
            Texture = CreateDustTexture(),
            ColorRamp = new Gradient
            {
                Offsets = [0, 0.58f, 1],
                Colors = [new Color("f3ca80d8"), new Color("d898408f"), new Color("85501900")]
            },
            ShowBehindParent = true
        };
        riders.AddChild(hoofDust);

        dustVeil = new ShaderMaterial { Shader = new Shader { Code = DustVeilShader } };
        AddChild(new ColorRect
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            MouseFilter = MouseFilterEnum.Ignore,
            Material = dustVeil
        });

        title = new Label
        {
            Text = "Дело сделано.",
            Position = new Vector2(276, 260),
            Size = new Vector2(600, 100),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Modulate = new Color(1, 1, 1, 0),
            MouseFilter = MouseFilterEnum.Ignore
        };
        title.AddThemeFontSizeOverride("font_size", 58);
        title.AddThemeColorOverride("font_color", new Color("2b1809"));
        title.AddThemeColorOverride("font_outline_color", new Color("e8c47f"));
        title.AddThemeConstantOverride("outline_size", 5);
        AddChild(title);

        resultPanel = new Panel
        {
            Position = new Vector2(196, 512),
            Size = new Vector2(760, 110),
            Modulate = new Color(1, 1, 1, 0)
        };
        resultPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.03f, 0.02f, 0.01f, 0.78f),
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
        var result = new Label
        {
            Position = new Vector2(18, 8),
            Size = new Vector2(724, 94),
            Text = $"Ты унёс из этого {TableMessages.LootAmount(PlayerShare)} ({PlayerShare}%).\n{TableMessages.LootReply(PlayerShare)}",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        result.AddThemeColorOverride("font_color", new Color("f5e5b8"));
        result.AddThemeFontSizeOverride("font_size", 20);
        resultPanel.AddChild(result);
        AddChild(resultPanel);
    }

    public override void _Process(double delta)
    {
        elapsed += (float)delta * 0.5f;
        Modulate = new Color(1, 1, 1, Mathf.SmoothStep(0, 1, Mathf.Clamp(elapsed / FadeInDuration, 0, 1)));

        var rideProgress = Mathf.SmoothStep(0, 1, Mathf.Clamp((elapsed - 0.35f) / 5.4f, 0, 1));
        var width = Mathf.Lerp(350, 58, rideProgress);
        var height = width * 1.058f;
        riders.Size = new Vector2(width, height);
        riders.Position = new Vector2(576 - width / 2, Mathf.Lerp(280, 310, rideProgress) + Mathf.Sin(elapsed * 10) * (1 - rideProgress) * 2.5f);

        hoofDust.Position = new Vector2(width / 2, height * 0.91f);
        hoofDust.EmissionRectExtents = new Vector2(width * 0.34f, 3);
        hoofDust.Scale = Vector2.One * Mathf.Lerp(1.7f, 0.35f, rideProgress);
        hoofDust.Emitting = elapsed >= 0.35f && rideProgress < 0.98f;

        var dustProgress = Mathf.Clamp((elapsed - 2.1f) / 5.3f, 0, 1);
        dustVeil.SetShaderParameter("progress", dustProgress);
        var titleAlpha = Mathf.SmoothStep(0, 1, Mathf.Clamp((elapsed - 7.25f) / 0.75f, 0, 1));
        title.Modulate = new Color(1, 1, 1, titleAlpha);
        resultPanel.Modulate = new Color(1, 1, 1, titleAlpha);
    }

    private static Texture2D CreateDustTexture() => new GradientTexture2D
    {
        Width = 32,
        Height = 32,
        Fill = GradientTexture2D.FillEnum.Radial,
        FillFrom = new Vector2(0.5f, 0.5f),
        FillTo = new Vector2(0.5f, 0),
        Gradient = new Gradient
        {
            Offsets = [0, 0.55f, 1],
            Colors = [Colors.White, new Color(1, 1, 1, 0.55f), Colors.Transparent]
        }
    };

    private static TextureRect FullRectTexture(string path) => new()
    {
        LayoutMode = 1,
        AnchorsPreset = (int)LayoutPreset.FullRect,
        AnchorRight = 1,
        AnchorBottom = 1,
        GrowHorizontal = GrowDirection.Both,
        GrowVertical = GrowDirection.Both,
        Texture = GD.Load<Texture2D>(path),
        ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
        StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
        MouseFilter = MouseFilterEnum.Ignore
    };
}
