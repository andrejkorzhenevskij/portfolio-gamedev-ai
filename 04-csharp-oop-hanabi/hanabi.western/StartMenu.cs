using Godot;

public partial class StartMenu : Control
{
    private const string BackgroundPath = "res://assets/title_frontier.png";
    private const string CloudPath = "res://assets/title_clouds.png";
    private const string StudioLogoPath = "res://assets/studio_logo.png";
    private const string IntroScenePath = "res://IntroSaloon.tscn";
    private const string CloudShader = """
        shader_type canvas_item;
        uniform sampler2D cloud_texture : repeat_enable, filter_linear;

        void fragment() {
            float layer_height = 0.667;
            vec2 sample_uv = vec2(UV.x + TIME * 0.012, UV.y / layer_height);
            vec4 clouds = texture(cloud_texture, sample_uv);
            COLOR = UV.y <= layer_height ? clouds : vec4(0.0);
        }
        """;
    private const string ForegroundShader = """
        shader_type canvas_item;
        uniform sampler2D scene_texture : filter_linear;

        void fragment() {
            vec4 scene = texture(scene_texture, UV);

            float terrain = smoothstep(0.29, 0.49, UV.y);

            float post_body =
                step(abs(UV.x - 0.198), 0.047) *
                step(0.245, UV.y) *
                (1.0 - step(0.91, UV.y));

            float cap_y = clamp((UV.y - 0.17) / 0.09, 0.0, 1.0);
            float post_cap =
                step(0.17, UV.y) *
                (1.0 - step(0.26, UV.y)) *
                step(abs(UV.x - 0.198), cap_y * 0.058);

            float foreground = max(terrain, max(post_body, post_cap));
            COLOR = vec4(scene.rgb, scene.a * foreground);
        }
        """;
    private const string DustShader = """
        shader_type canvas_item;

        float puff(vec2 point, vec2 center, float radius) {
            float wobble = sin(point.x * 130.0 + TIME * 0.8) * radius * 0.12;
            return 1.0 - smoothstep(radius * 0.52, radius, distance(point, center) + wobble);
        }

        float drifting_puff(vec2 point, float offset, float side) {
            float life = fract(TIME / 5.2 + offset);
            vec2 center = vec2(0.598 + side * life * 0.055, 0.558 - life * 0.035);
            float shape = puff(point, center, 0.012 + life * 0.036);
            return shape * sin(life * 3.14159265) * (1.0 - life * 0.45);
        }

        void fragment() {
            float dust = drifting_puff(UV, 0.00, -1.0)
                       + drifting_puff(UV, 0.18,  1.0)
                       + drifting_puff(UV, 0.37, -0.7)
                       + drifting_puff(UV, 0.58,  0.8)
                       + drifting_puff(UV, 0.79, -0.5);
            COLOR = vec4(vec3(0.90, 0.72, 0.42), dust * 0.62);
        }
        """;

    private Control studioSplash = null!;
    private Label title = null!;
    private Label subtitle = null!;
    private Button firstLanguageButton = null!;
    private HBoxContainer menuButtons = null!;
    private int revealStep;
    private bool studioVisible = true;

    public override void _Ready()
    {
        BuildTitleScreen();
        BuildStudioSplash();
        _ = EndStudioSplashAfterDelay();
    }

    public override void _Input(InputEvent @event)
    {
        var activated =
            @event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } ||
            @event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Enter or Key.KpEnter or Key.Space };

        if (!activated)
        {
            return;
        }

        var handled = false;
        if (studioVisible)
        {
            EndStudioSplash();
            handled = true;
        }
        else if (revealStep < 3)
        {
            RevealNext();
            handled = true;
        }

        if (handled)
        {
            GetViewport().SetInputAsHandled();
        }
    }

    private void BuildTitleScreen()
    {
        AddChild(FullRectTexture(BackgroundPath));

        var cloudMaterial = new ShaderMaterial
        {
            Shader = new Shader { Code = CloudShader }
        };
        cloudMaterial.SetShaderParameter("cloud_texture", GD.Load<Texture2D>(CloudPath));
        AddChild(new ColorRect
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            MouseFilter = MouseFilterEnum.Ignore,
            Material = cloudMaterial
        });

        var foregroundMaterial = new ShaderMaterial
        {
            Shader = new Shader { Code = ForegroundShader }
        };
        foregroundMaterial.SetShaderParameter("scene_texture", GD.Load<Texture2D>(BackgroundPath));
        AddChild(new ColorRect
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            MouseFilter = MouseFilterEnum.Ignore,
            Material = foregroundMaterial
        });

        AddChild(new ColorRect
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            MouseFilter = MouseFilterEnum.Ignore,
            Material = new ShaderMaterial { Shader = new Shader { Code = DustShader } }
        });

        title = CreateTitleLabel("THE LAST JOB", new Vector2(126, 74), new Vector2(900, 78), 54);
        subtitle = CreateTitleLabel("HANABI WESTERN", new Vector2(276, 150), new Vector2(600, 48), 25);
        title.Visible = false;
        subtitle.Visible = false;
        AddChild(title);
        AddChild(subtitle);

        menuButtons = new HBoxContainer
        {
            Position = new Vector2(408, 548),
            Size = new Vector2(336, 54),
            Visible = false
        };
        menuButtons.AddThemeConstantOverride("separation", 8);

        firstLanguageButton = CreateMenuButton("EN", () => StartGame("en"));
        menuButtons.AddChild(firstLanguageButton);
        menuButtons.AddChild(CreateMenuButton("RU", () => StartGame("ru")));
        AddChild(menuButtons);
    }

    private void BuildStudioSplash()
    {
        studioSplash = new Control
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            MouseFilter = MouseFilterEnum.Ignore
        };
        studioSplash.AddChild(new ColorRect
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            Color = Colors.Black,
            MouseFilter = MouseFilterEnum.Ignore
        });
        studioSplash.AddChild(new TextureRect
        {
            Position = new Vector2(326, 74),
            Size = new Vector2(500, 500),
            Texture = GD.Load<Texture2D>(StudioLogoPath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore
        });
        AddChild(studioSplash);
    }

    private async System.Threading.Tasks.Task EndStudioSplashAfterDelay()
    {
        await ToSignal(GetTree().CreateTimer(1.25), SceneTreeTimer.SignalName.Timeout);
        EndStudioSplash();
    }

    private void EndStudioSplash()
    {
        if (!studioVisible)
        {
            return;
        }

        studioVisible = false;
        var tween = CreateTween();
        tween.TweenProperty(studioSplash, "modulate:a", 0f, 0.3);
        tween.TweenCallback(Callable.From(studioSplash.QueueFree));
    }

    private void RevealNext()
    {
        revealStep++;
        switch (revealStep)
        {
            case 1:
                Reveal(title);
                break;
            case 2:
                Reveal(subtitle);
                break;
            case 3:
                Reveal(menuButtons);
                firstLanguageButton.GrabFocus();
                break;
        }
    }

    private void Reveal(CanvasItem item)
    {
        item.Visible = true;
        item.Modulate = new Color(1, 1, 1, 0);
        CreateTween().TweenProperty(item, "modulate:a", 1f, 0.32);
    }

    private void StartGame(string locale)
    {
        GameText.Use(locale);
        GetTree().ChangeSceneToFile(IntroScenePath);
    }

    private static TextureRect FullRectTexture(string texturePath)
    {
        return new TextureRect
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            Texture = GD.Load<Texture2D>(texturePath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            MouseFilter = MouseFilterEnum.Ignore
        };
    }

    private static Label CreateTitleLabel(string text, Vector2 position, Vector2 size, int fontSize)
    {
        var label = new Label
        {
            Text = text,
            Position = position,
            Size = size,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", new Color("f0cf83"));
        label.AddThemeColorOverride("font_outline_color", new Color("2b1809"));
        label.AddThemeConstantOverride("outline_size", 8);
        return label;
    }

    private static StyleBoxFlat CreateButtonStyle(Color color)
    {
        return new StyleBoxFlat
        {
            BgColor = color,
            BorderColor = new Color("4a2b14"),
            BorderWidthLeft = 3,
            BorderWidthTop = 3,
            BorderWidthRight = 3,
            BorderWidthBottom = 3,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusBottomLeft = 4,
            ShadowColor = new Color(0, 0, 0, 0.45f),
            ShadowSize = 6
        };
    }

    private static Button CreateMenuButton(string text, Action pressed)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(164, 54),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            FocusMode = FocusModeEnum.All
        };
        button.AddThemeFontSizeOverride("font_size", 17);
        button.AddThemeColorOverride("font_color", new Color("382311"));
        button.AddThemeColorOverride("font_hover_color", new Color("241508"));
        button.AddThemeStyleboxOverride("normal", CreateButtonStyle(new Color("d9b974")));
        button.AddThemeStyleboxOverride("hover", CreateButtonStyle(new Color("efd28e")));
        button.AddThemeStyleboxOverride("pressed", CreateButtonStyle(new Color("b9924f")));
        button.Pressed += pressed;
        return button;
    }
}
