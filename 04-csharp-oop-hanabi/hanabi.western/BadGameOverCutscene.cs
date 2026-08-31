using Godot;

public partial class BadGameOverCutscene : Control
{
    private const string MainScenePath = "res://Main.tscn";
    private const string BridgePath = "res://assets/bad_gameover_bridge.png";
    private const string BrokenBridgePath = "res://assets/bad_gameover_bridge_broken.png";
    private const string EscapePath = "res://assets/bad_gameover_escape.png";
    private const string OutlawPath = "res://assets/bad_gameover_outlaw.png";
    private const string SoldiersPath = "res://assets/bad_gameover_soldiers.png";
    private const string ExplosionPath = "res://assets/bad_gameover_explosion.png";
    private const float FadeInDuration = 1.2f;
    private const float BlastStart = 2.90f;

    private readonly (float Time, int Soldier)[] shots =
    [
        (5.20f, 0), (5.55f, 2), (5.97f, 1), (6.47f, 3),
        (7.07f, 0), (7.75f, 2)
    ];

    private Control sceneLayer = null!;
    private TextureRect intactBridge = null!;
    private TextureRect brokenBridge = null!;
    private TextureRect escape = null!;
    private TextureRect outlaw = null!;
    private TextureRect soldiers = null!;
    private TextureRect explosion = null!;
    private ColorRect blastFlash = null!;
    private ColorRect smokeVeil = null!;
    private ColorRect endingShade = null!;
    private Label failureTitle = null!;
    private Button retryButton = null!;
    private readonly List<Polygon2D> muzzleFlashes = [];
    private float elapsed;
    private bool retryShown;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        ZIndex = 1000;
        Modulate = new Color(1, 1, 1, 0);

        sceneLayer = new Control
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(sceneLayer);

        intactBridge = FullRectTexture(BridgePath);
        brokenBridge = FullRectTexture(BrokenBridgePath);
        brokenBridge.Visible = false;
        sceneLayer.AddChild(intactBridge);
        sceneLayer.AddChild(brokenBridge);

        escape = Actor(EscapePath, 180);
        outlaw = Actor(OutlawPath, 155);
        soldiers = Actor(SoldiersPath, 280);
        outlaw.Scale = Vector2.One * 0.403f;
        soldiers.Scale = Vector2.One * 0.403f;
        explosion = Actor(ExplosionPath, 260);
        outlaw.Visible = false;
        soldiers.Visible = false;
        explosion.Visible = false;
        sceneLayer.AddChild(escape);
        sceneLayer.AddChild(outlaw);
        sceneLayer.AddChild(soldiers);
        AddMuzzleFlashes();
        sceneLayer.AddChild(explosion);

        blastFlash = FullRectColor(new Color("ffc46900"));
        smokeVeil = FullRectColor(new Color("9e6a2d00"));
        endingShade = FullRectColor(new Color(0.035f, 0.02f, 0.01f, 0));
        AddChild(blastFlash);
        AddChild(smokeVeil);
        AddChild(endingShade);

        failureTitle = new Label
        {
            Text = "Не удалось",
            Position = new Vector2(326, 235),
            Size = new Vector2(500, 86),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Modulate = new Color(1, 1, 1, 0),
            MouseFilter = MouseFilterEnum.Ignore
        };
        failureTitle.AddThemeFontSizeOverride("font_size", 58);
        failureTitle.AddThemeColorOverride("font_color", new Color("f3d392"));
        failureTitle.AddThemeColorOverride("font_outline_color", new Color("241307"));
        failureTitle.AddThemeConstantOverride("outline_size", 8);
        AddChild(failureTitle);

        retryButton = new Button
        {
            Text = "Ещё раз?",
            Position = new Vector2(476, 344),
            Size = new Vector2(200, 58),
            Visible = false,
            Disabled = true
        };
        retryButton.AddThemeFontSizeOverride("font_size", 22);
        retryButton.AddThemeColorOverride("font_color", new Color("382311"));
        retryButton.AddThemeStyleboxOverride("normal", ButtonStyle(new Color("d9b974")));
        retryButton.AddThemeStyleboxOverride("hover", ButtonStyle(new Color("efd28e")));
        retryButton.AddThemeStyleboxOverride("pressed", ButtonStyle(new Color("b9924f")));
        retryButton.ZIndex = 110;
        retryButton.Pressed += () =>
        {
            QueueFree();
            GetTree().ChangeSceneToFile(MainScenePath);
        };
        AddChild(retryButton);
        AddChild(new CutsceneText { TextPath = "res://texts/mistake-3.md" });
    }

    public override void _Process(double delta)
    {
        elapsed += (float)delta * 0.5f;
        Modulate = new Color(1, 1, 1, Mathf.Clamp(elapsed / FadeInDuration, 0, 1));

        var escapeProgress = Mathf.Clamp((elapsed - 0.35f) / 3.75f, 0, 1);
        var escapeEase = Mathf.SmoothStep(0, 1, escapeProgress);
        var escapeSize = Mathf.Lerp(180, 72, escapeEase);
        escape.Size = Vector2.One * escapeSize;
        escape.PivotOffset = Vector2.One * escapeSize / 2;
        escape.Rotation = Mathf.DegToRad(-20);
        escape.Position = new Vector2(576 - escapeSize / 2, Mathf.Lerp(545, 92, escapeEase));
        escape.Visible = elapsed >= 0.35f && elapsed < BlastStart + 0.2f;

        var blastProgress = Mathf.Clamp((elapsed - BlastStart) / 1.85f, 0, 1);
        explosion.Visible = blastProgress is > 0 and < 1;
        if (explosion.Visible)
        {
            var explosionSize = Mathf.Lerp(260, 1250, Mathf.SmoothStep(0, 1, blastProgress));
            explosion.Size = Vector2.One * explosionSize;
            explosion.Position = new Vector2(576 - explosionSize / 2, 324 - explosionSize / 2);
            var explosionAlpha = blastProgress < 0.18f
                ? blastProgress / 0.18f
                : 1 - Mathf.SmoothStep(0.55f, 1, blastProgress);
            explosion.Modulate = new Color(1, 1, 1, explosionAlpha);
        }

        brokenBridge.Visible = elapsed >= BlastStart + 0.63f;
        intactBridge.Visible = !brokenBridge.Visible;
        var flashAlpha = Mathf.Max(0, 1 - Mathf.Abs(elapsed - (BlastStart + 0.25f)) / 0.18f) * 0.48f;
        blastFlash.Color = new Color("ffc469") with { A = flashAlpha };
        smokeVeil.Color = new Color("9e6a2d") with { A = Mathf.Sin(blastProgress * Mathf.Pi) * 0.42f };

        var cameraProgress = Mathf.SmoothStep(0, 1, Mathf.Clamp((elapsed - (BlastStart + 0.13f)) / 2.05f, 0, 1));
        var cameraScale = Mathf.Lerp(1, 1.35f, cameraProgress);
        sceneLayer.Scale = Vector2.One * cameraScale;
        sceneLayer.Position = Vector2.Zero.Lerp(new Vector2(-390, -220), cameraProgress);

        var standoffAlpha = Mathf.SmoothStep(0, 1, Mathf.Clamp((elapsed - (BlastStart + 1.47f)) / 0.48f, 0, 1));
        outlaw.Visible = elapsed >= BlastStart + 1.47f;
        soldiers.Visible = outlaw.Visible;
        outlaw.Position = new Vector2(667, Mathf.Lerp(445, 400, standoffAlpha));
        soldiers.Position = new Vector2(631, Mathf.Lerp(570, 525, standoffAlpha));
        outlaw.SelfModulate = Colors.Black with { A = standoffAlpha };
        soldiers.SelfModulate = Colors.Black with { A = standoffAlpha };

        foreach (var muzzleFlash in muzzleFlashes)
        {
            muzzleFlash.Visible = false;
        }
        foreach (var shot in shots)
        {
            if (Mathf.Abs(elapsed - shot.Time) < 0.065f)
            {
                muzzleFlashes[shot.Soldier].Visible = true;
            }
        }

        var endingProgress = Mathf.SmoothStep(0, 1, Mathf.Clamp((elapsed - BlastStart) / (10.2f - BlastStart), 0, 1));
        endingShade.Color = new Color(0.035f, 0.02f, 0.01f, endingProgress * 0.84f);
        var titleAlpha = Mathf.SmoothStep(0, 1, Mathf.Clamp((elapsed - 8.15f) / 1.05f, 0, 1));
        failureTitle.Modulate = new Color(1, 1, 1, titleAlpha);

        if (!retryShown && elapsed >= 10.2f)
        {
            retryShown = true;
            retryButton.Visible = true;
            retryButton.Disabled = false;
            retryButton.GrabFocus();
        }
    }

    private void AddMuzzleFlashes()
    {
        var positions = new[]
        {
            new Vector2(124, 31), new Vector2(151, 31),
            new Vector2(122, 129), new Vector2(151, 130)
        };
        foreach (var position in positions)
        {
            var flash = new Polygon2D
            {
                Position = position,
                Polygon =
                [
                    new Vector2(0, -16), new Vector2(4, -4), new Vector2(16, 0), new Vector2(4, 4),
                    new Vector2(0, 16), new Vector2(-4, 4), new Vector2(-16, 0), new Vector2(-4, -4)
                ],
                Color = Colors.White,
                Visible = false
            };
            muzzleFlashes.Add(flash);
            soldiers.AddChild(flash);
        }
    }

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

    private static TextureRect Actor(string path, float size) => new()
    {
        Size = Vector2.One * size,
        Texture = GD.Load<Texture2D>(path),
        ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
        StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
        MouseFilter = MouseFilterEnum.Ignore
    };

    private static ColorRect FullRectColor(Color color) => new()
    {
        LayoutMode = 1,
        AnchorsPreset = (int)LayoutPreset.FullRect,
        AnchorRight = 1,
        AnchorBottom = 1,
        GrowHorizontal = GrowDirection.Both,
        GrowVertical = GrowDirection.Both,
        Color = color,
        MouseFilter = MouseFilterEnum.Ignore
    };

    private static StyleBoxFlat ButtonStyle(Color color) => new()
    {
        BgColor = color,
        BorderColor = new Color("4a2b12"),
        BorderWidthLeft = 2,
        BorderWidthTop = 2,
        BorderWidthRight = 2,
        BorderWidthBottom = 2,
        CornerRadiusTopLeft = 5,
        CornerRadiusTopRight = 5,
        CornerRadiusBottomLeft = 5,
        CornerRadiusBottomRight = 5
    };
}
