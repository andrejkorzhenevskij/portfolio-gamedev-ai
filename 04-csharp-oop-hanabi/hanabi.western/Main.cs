using Godot;
using Hanabi.Core;

public partial class Main : Control
{
    private const int MaxMistakes = 3;
    private const int MaxHintTokens = 8;
    private const int AdviceCostStep = 10;
    private const int VisibleDiscardCards = 12;
    private const double CardFlightDuration = 0.55;
    private const int LayoutSeparation = 8;
    private const int PartnerAreaHeight = 118;
    private const int CenterRowHeight = 236;
    private const int PlanLaneSpacing = 6;
    private const float PlanAreaWidth = 630f;
    private const float PartnerBreathPixels = 4f;
    private const float PartnerBreathPeriod = 5.8f;
    private const string LogPath = "hanabi_log.txt";
    private const string SaloonBackgroundTexturePath = "res://assets/saloon_background.png";
    private const string TableTexturePath = "res://assets/table.png";
    private const string CardBackTexturePath = "res://assets/card_back.png";
    private const string DeckTexturePath = "res://assets/deck.png";
    private const string HintTokenTexturePath = "res://assets/token_engraved.png";
    private const string RevolverDrumTexturePath = "res://assets/revolver_drum.png";
    private const string WhiskeyFullTexturePath = "res://assets/whiskey_full_engraved.png";
    private const string WhiskeyEmptyTexturePath = "res://assets/whiskey_empty_engraved.png";
    private const string GlassFullTexturePath = "res://assets/glass_full_engraved.png";
    private const string GlassEmptyTexturePath = "res://assets/glass_empty_engraved.png";
    private const string PartnerFigureTexturePath = "res://assets/partner_stranger_engraved.png";
    private const string SceneShadeShader = """
        shader_type canvas_item;
        void fragment() {
            vec2 p = UV - vec2(0.5);
            float edge = smoothstep(0.28, 0.78, length(p));
            float bottom = smoothstep(0.48, 1.0, UV.y) * 0.05;
            float sway = sin(TIME * 0.74) * 0.55 + sin(TIME * 0.63 + 1.7) * 0.30;
            float tremble = sin(TIME * 2.4 + sin(TIME * 0.51) * 1.8) * 0.08;
            float flicker = sway + tremble;
            vec2 lamp_center = vec2(
                0.58 + sin(TIME * 0.39) * 0.010,
                0.27 + sin(TIME * 0.47 + 0.8) * 0.008);
            vec2 lamp_delta = (UV - lamp_center) * vec2(0.78, 1.0);
            float lamp = 1.0 - smoothstep(0.10, 0.68, length(lamp_delta));
            float shade = 0.11 + edge * 0.12 + bottom - lamp * (0.035 + flicker * 0.018);
            vec3 tint = mix(vec3(0.055, 0.035, 0.018), vec3(0.12, 0.065, 0.022), lamp * 0.24);
            COLOR = vec4(tint, clamp(shade, 0.025, 0.30));
        }
        """;
    private const string PassingShadowShader = """
        shader_type canvas_item;

        float random_value(float value) {
            return fract(sin(value * 12.9898) * 43758.5453);
        }

        float ellipse(vec2 point, vec2 center, vec2 radius) {
            return length((point - center) / radius);
        }

        void fragment() {
            float cycle_length = 29.0;
            float cycle = floor(TIME / cycle_length);
            float phase = fract(TIME / cycle_length);
            float start = 0.05 + random_value(cycle + 2.0) * 0.20;
            float duration = 0.12;
            float progress = clamp((phase - start) / duration, 0.0, 1.0);
            float active = step(start, phase) * step(phase, start + duration);
            float direction = step(0.5, random_value(cycle + 7.0));
            float travel = mix(progress, 1.0 - progress, direction);
            float center_x = mix(-0.22, 1.22, travel);

            vec2 point = UV;
            point.x += (point.y - 0.20) * mix(-0.08, 0.08, direction);
            float hat = 1.0 - smoothstep(0.80, 1.28, ellipse(point, vec2(center_x, 0.105), vec2(0.115, 0.026)));
            float crown = 1.0 - smoothstep(0.82, 1.30, ellipse(point, vec2(center_x, 0.145), vec2(0.060, 0.065)));
            float head = 1.0 - smoothstep(0.82, 1.30, ellipse(point, vec2(center_x, 0.215), vec2(0.050, 0.090)));
            float shoulders = 1.0 - smoothstep(0.78, 1.32, ellipse(point, vec2(center_x, 0.395), vec2(0.205, 0.220)));
            float torso = 1.0 - smoothstep(0.80, 1.35, ellipse(point, vec2(center_x, 0.610), vec2(0.150, 0.350)));
            float silhouette = max(max(hat, crown), max(head, max(shoulders, torso)));
            float envelope = sin(progress * 3.14159265) * active;
            float lower_fade = 1.0 - smoothstep(0.58, 0.78, UV.y);
            COLOR = vec4(vec3(0.025, 0.014, 0.008), silhouette * envelope * lower_fade * 0.30);
        }
        """;
    private static readonly Vector2 ScreenSize = new(1152, 648);
    private static readonly Vector2 TableTextureOffset = new(48, 0);
    private static readonly Vector2 LayoutPadding = new(18, 10);
    private static readonly Vector2 PlanAreaPosition = new(380, 0);
    private static readonly Vector2 StockAreaPosition = new(998, 0);
    private static readonly Vector2 PartnerFigurePosition = new(296, -122);
    private static readonly Vector2 PartnerFigureSize = new(560, 374);
    private static readonly Vector2 CardSize = new(72, 96);
    private const float HandTableOffset = 56f;
    private static readonly Vector2 PartnerCardSize = new(68, 90);
    private static readonly Vector2 PlayerCardSize = new(120, 158);
    private static readonly Vector2 PlayerHandFanSize = new(680, 190);
    private static readonly Vector2 PlayerHandFanStart = new(82, 2);
    private static readonly Vector2 DiscardPilePosition = new(812, 470);
    private static readonly Vector2 DiscardPileSize = new(330, 150);
    private static readonly Vector2 DiscardCardSize = new(82, 135);
    private static readonly Vector2 ShareNoteSize = new(156, 96);
    private static readonly Vector2 PlanLaneSize = new(116, 230);
    private static readonly Vector2 PlayedCardSize = new(75, 123);
    private static readonly Vector2 StockDeckSize = new(148, 102);
    private static readonly Vector2 StockTokenSize = new(84, 84);
    private static readonly Vector2 OverlayCardSize = new(250, 412);
    private static readonly Vector2 PartnerAdviceCardSize = new(106, 140);
    private static readonly Vector2 MistakeTokensSize = new(520, 470);
    private static readonly Vector2 MistakeBottleSize = new(346, 346);
    private static readonly Vector2 MistakeGlassSize = new(156, 156);
    private static readonly Vector2 FirstMistakeZoomTarget = new(340, 180);
    private static readonly Vector2 SecondMistakeZoomTarget = new(228, 180);
    private static readonly Vector2 LastMistakeZoomTarget = new(293, 501);
    private static readonly string[] RussianPlanLanes = ["Оружие", "Наводка", "Охрана", "Цель", "Отход"];
    private static readonly string[] EnglishPlanLanes = ["Guns", "The Lead", "Escort", "The Mark", "Getaway"];
    private static string[] PlanLanes => GameText.English ? EnglishPlanLanes : RussianPlanLanes;
    private static readonly CardColor[] LaneColors =
    [
        CardColor.Red,
        CardColor.Green,
        CardColor.Blue,
        CardColor.Yellow,
        CardColor.White
    ];

    private Hanabi.Core.Game game = null!;
    private readonly int[] laneProgress = new int[5];
    private readonly int?[] burnedFromRank = new int?[5];
    private readonly List<string> actionLog = [];
    private readonly List<CardFlight> cardFlights = [];
    private readonly List<Card> discardedCards = [];
    private readonly Random random = new();
    private int mistakesLeft = MaxMistakes;
    private int hintTokens = MaxHintTokens;
    private int playerShare = 50;
    private int partnerShare = 50;
    private int adviceCost = AdviceCostStep;
    private int turnNumber = 1;
    private int currentPlayerIndex = 0;
    private string lastSelected = GameText.Pick("Нет", "None");
    private string lastAction = TableMessages.ChooseAction;
    private string lastResult = TableMessages.Waiting;
    private string playerHandMessage = "";
    private int? selectedHandIndex;
    private int? selectedCardIndex;
    private bool cardOverlayVisible;
    private bool partnerAdviceOverlayVisible;
    private bool mistakeOneCutscenePending;
    private bool mistakeTwoCutscenePending;
    private bool badGameOverCutscenePending;
    private bool goodGameOverCutscenePending;
    private bool cutscenePlaying;
    private bool rulesVisible = true;
    private bool guideVisible;
    private string? guidePopupId;
    private string? viewedCardTexturePath;
    private int? partnerAdviceCardIndex;
    private Card? partnerAdviceCard;
    private int? hintedHandIndex;
    private CardColor? hintedColor;
    private int? hintedRank;

    public override void _Ready()
    {
        game = new Hanabi.Core.Game(CreateStartingCards());
        AddLog("Prototype started");
        RefreshTable();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (cutscenePlaying || rulesVisible || guideVisible)
        {
            return;
        }

        if (@event is not InputEventKey { Pressed: true, Echo: false } key)
        {
            return;
        }

        if (key.Keycode is Key.Enter or Key.KpEnter or Key.Space)
        {
            PlaySelected();
            GetViewport().SetInputAsHandled();
        }
        else if (key.Keycode is Key.Backspace or Key.Delete)
        {
            DropSelected();
            GetViewport().SetInputAsHandled();
        }
    }

    private void RefreshTable()
    {
        cardFlights.RemoveAll(flight => flight.IsDone);

        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }

        AddChild(CreateTextureLayer(SaloonBackgroundTexturePath, Vector2.Zero));
        AddChild(CreatePartnerFigureLayer());
        AddChild(CreateTextureLayer(TableTexturePath, TableTextureOffset));
        AddChild(CreatePassingShadow());

        var layoutSize = ScreenSize - LayoutPadding * 2;
        var layout = new VBoxContainer
        {
            Position = LayoutPadding,
            CustomMinimumSize = layoutSize,
            Size = layoutSize
        };
        layout.AddThemeConstantOverride("separation", LayoutSeparation);
        AddChild(layout);

        layout.AddChild(CreatePartnerHandArea());
        layout.AddChild(CreateCenterRow());
        layout.AddChild(CreatePlayerHandArea());
        AddChild(CreateSceneShade());
        AddChild(CreateActionLogArea());
        AddChild(CreateAdviceBubble());
        AddChild(CreateNavigation());
        AddChild(CreateShareCounter());
        AddChild(CreateDiscardPileArea());
        if (cardFlights.Count > 0)
        {
            AddChild(CreateCardFlightLayer());
        }

        if (partnerAdviceOverlayVisible)
        {
            AddChild(CreatePartnerAdviceOverlay());
        }

        if (cardOverlayVisible)
        {
            AddChild(CreateCardOverlay());
        }
        if (viewedCardTexturePath is not null)
        {
            AddChild(CreateViewedCardOverlay());
        }
        if (rulesVisible)
        {
            AddChild(CreateRulesOverlay());
        }
        if (guideVisible)
        {
            AddChild(CreateGuideOverlay());
        }

        StartPendingMistakeCutscene();
    }

    private Card[] CreateStartingCards()
    {
        var cards =
            (from rank in Enumerable.Range(1, 5)
             from color in LaneColors
             from _ in Enumerable.Range(0, rank == 1 ? 3 : rank == 5 ? 1 : 2)
             select new Card(color, rank)).ToArray();

        random.Shuffle(cards);
        return cards;
    }

    private Control CreatePartnerHandArea()
    {
        var area = CreateZone("PartnerHand", new Vector2(1, PartnerAreaHeight));
        area.AddChild(new Control { CustomMinimumSize = new Vector2(1, 24) });
        AddHand(area, game.Hands[1], handIndex: 1, hideCards: false);
        return area;
    }

    private static TextureRect CreateTextureLayer(string texturePath, Vector2 position)
    {
        return new TextureRect
        {
            Position = position,
            Size = ScreenSize,
            Texture = GD.Load<Texture2D>(texturePath),
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            MouseFilter = MouseFilterEnum.Ignore
        };
    }

    private static TextureRect CreatePartnerFigureLayer()
    {
        return new BreathingFigure
        {
            Position = PartnerFigurePosition,
            Size = PartnerFigureSize,
            Texture = GD.Load<Texture2D>(PartnerFigureTexturePath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore,
            PivotOffset = new Vector2(PartnerFigureSize.X / 2f, PartnerFigureSize.Y)
        };
    }

    private static ColorRect CreateSceneShade()
    {
        return new ColorRect
        {
            Size = ScreenSize,
            Color = Colors.White,
            Material = new ShaderMaterial { Shader = new Shader { Code = SceneShadeShader } },
            MouseFilter = MouseFilterEnum.Ignore
        };
    }

    private static ColorRect CreatePassingShadow()
    {
        return new ColorRect
        {
            Size = ScreenSize,
            Color = Colors.White,
            Material = new ShaderMaterial { Shader = new Shader { Code = PassingShadowShader } },
            MouseFilter = MouseFilterEnum.Ignore
        };
    }

    private Control CreateCenterRow()
    {
        var row = new Control
        {
            CustomMinimumSize = new Vector2(1, CenterRowHeight),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };

        var mistakes = CreateMistakesArea();
        mistakes.Position = new Vector2(78, -34);
        row.AddChild(mistakes);

        var plan = CreatePlanArea();
        plan.Position = PlanAreaPosition;
        row.AddChild(plan);

        var deck = CreateDeckArea();
        deck.Position = StockAreaPosition;
        row.AddChild(deck);
        return row;
    }

    private Control CreatePlayerHandArea()
    {
        var area = CreateZone("PlayerHand", new Vector2(1, 170));
        if (!IsGameOver() && !string.IsNullOrWhiteSpace(playerHandMessage))
        {
            var message = new Label
            {
                Text = playerHandMessage,
                HorizontalAlignment = HorizontalAlignment.Center,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            message.AddThemeColorOverride("font_color", new Color("f5e5b8"));
            message.AddThemeFontSizeOverride("font_size", 18);
            area.AddChild(message);
        }

        if (!IsGameOver())
        {
            AddHand(area, game.Hands[0], handIndex: 0, hideCards: true);
        }

        return area;
    }

    private Control CreateMistakesArea()
    {
        var area = CreateZone("Mistakes", new Vector2(370, 1), expand: false);
        area.AddChild(CreateMistakeTokens());
        return area;
    }

    private Control CreatePlanArea()
    {
        var area = CreateZone("Plan", new Vector2(630, 1), expand: false);

        var lanes = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        lanes.AddThemeConstantOverride("separation", PlanLaneSpacing);

        for (var lane = 0; lane < PlanLanes.Length; lane++)
        {
            lanes.AddChild(CreatePlanLane(lane));
        }

        area.AddChild(lanes);
        return area;
    }

    private Control CreateDeckArea()
    {
        var area = new Control
        {
            Name = "Deck",
            CustomMinimumSize = new Vector2(156, 286),
            Size = new Vector2(156, 286),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter
        };

        var deck = CreateDeckPile();
        deck.Position = new Vector2(0, 0);
        area.AddChild(deck);

        var hints = CreateHintTokens(hintTokens);
        hints.Position = new Vector2(14, 114);
        area.AddChild(hints);

        return area;
    }

    private Control CreateActionLogArea()
    {
        var label = new Label
        {
            Position = new Vector2(0, 608),
            Size = new Vector2(ScreenSize.X, 32),
            Text = $"{lastAction} | {lastResult}",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
            ZIndex = 2
        };
        label.AddThemeColorOverride("font_color", IsGameOver() ? new Color("ffb08f") : new Color("f5e5b8"));
        label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.82f));
        label.AddThemeConstantOverride("outline_size", 4);
        label.AddThemeFontSizeOverride("font_size", 16);
        return label;
    }

    private Control CreateNavigation()
    {
        var row = new HBoxContainer
        {
            Position = new Vector2(30, 592),
            Size = new Vector2(188, 42),
            ZIndex = 55
        };
        row.AddThemeConstantOverride("separation", 8);
        row.AddChild(CreateActionButton(GameText.Pick("Правила", "Rules"), true, () =>
        {
            rulesVisible = true;
            guideVisible = false;
            RefreshTable();
        }));
        row.AddChild(CreateActionButton(GameText.Pick("Стол", "Table"), true, () =>
        {
            guideVisible = true;
            rulesVisible = false;
            RefreshTable();
        }));
        return row;
    }

    private Control CreateRulesOverlay()
    {
        var overlay = CreateDarkOverlay();
        var panel = new Panel
        {
            Position = new Vector2(34, 8),
            Size = new Vector2(1084, 632)
        };
        panel.AddThemeStyleboxOverride("panel", PanelStyle(new Color("2f2a24"), new Color("d6c07a")));
        overlay.AddChild(panel);
        panel.AddChild(CreateCloseButton(new Vector2(1036, 10), () =>
        {
            rulesVisible = false;
            RefreshTable();
        }));

        var title = new Label
        {
            Position = new Vector2(0, 18),
            Size = new Vector2(1084, 34),
            Text = GameText.Pick("Правила", "Rules"),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeColorOverride("font_color", new Color("f5e5b8"));
        title.AddThemeFontSizeOverride("font_size", 26);
        panel.AddChild(title);

        var text = new Label
        {
            Position = new Vector2(56, 62),
            Size = new Vector2(972, 550),
            Text = ReadMarkdownText(GameText.Path("rules.md")),
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            VerticalAlignment = VerticalAlignment.Top
        };
        text.AddThemeColorOverride("font_color", Colors.White);
        text.AddThemeFontSizeOverride("font_size", 18);
        panel.AddChild(text);
        return overlay;
    }

    private Control CreateGuideOverlay()
    {
        var overlay = CreateDarkOverlay();
        var sections = ReadGuideSections();
        overlay.AddChild(CreateCloseButton(new Vector2(1100, 14), () =>
        {
            guideVisible = false;
            guidePopupId = null;
            RefreshTable();
        }));
        overlay.AddChild(CreateCallout("partner_hand", GameText.Pick("рука партнёра", "partner's hand"), new Vector2(406, 78), new Vector2(170, 42)));
        overlay.AddChild(CreateCallout("plan_lanes", GameText.Pick("план", "plan"), new Vector2(576, 252), new Vector2(132, 42)));
        overlay.AddChild(CreateCallout("mistakes", GameText.Pick("ошибки", "mistakes"), new Vector2(106, 214), new Vector2(132, 42)));
        overlay.AddChild(CreateCallout("draw_deck", GameText.Pick("колода", "deck"), new Vector2(964, 164), new Vector2(132, 42)));
        overlay.AddChild(CreateCallout("hint_tokens", GameText.Pick("жетоны", "tokens"), new Vector2(944, 304), new Vector2(132, 42)));
        overlay.AddChild(CreateCallout("share_note", GameText.Pick("доли", "the cut"), new Vector2(932, 486), new Vector2(132, 42)));
        overlay.AddChild(CreateCallout("discard_pile", GameText.Pick("отбой", "discards"), new Vector2(790, 554), new Vector2(132, 42)));
        overlay.AddChild(CreateCallout("player_hand", GameText.Pick("ваша рука", "your hand"), new Vector2(384, 524), new Vector2(150, 42)));
        overlay.AddChild(CreateCallout("ask_advice", GameText.Pick("совет", "advice"), new Vector2(42, 134), new Vector2(132, 42)));
        if (guidePopupId is not null && sections.TryGetValue(guidePopupId, out var section))
        {
            overlay.AddChild(CreateGuidePopup(section.Title, section.Text));
        }
        return overlay;
    }

    private static ColorRect CreateDarkOverlay()
    {
        return new ColorRect
        {
            Size = ScreenSize,
            Color = new Color(0, 0, 0, 0.46f),
            MouseFilter = MouseFilterEnum.Stop,
            ZIndex = 100
        };
    }

    private Button CreateCloseButton(Vector2 position, Action close)
    {
        var button = CreateActionButton("X", true, close);
        button.Position = position;
        button.Size = new Vector2(34, 34);
        return button;
    }

    private Button CreateCallout(string id, string text, Vector2 position, Vector2 size)
    {
        var button = new Button
        {
            Position = position,
            Size = size,
            Text = text,
            FocusMode = FocusModeEnum.None
        };
        button.AddThemeColorOverride("font_color", new Color("2e2117"));
        button.AddThemeFontSizeOverride("font_size", 16);
        button.AddThemeStyleboxOverride("normal", PanelStyle(new Color("f0dca8"), new Color("7d5a35")));
        button.AddThemeStyleboxOverride("hover", PanelStyle(new Color("fff0c8"), new Color("7d5a35")));
        button.AddThemeStyleboxOverride("pressed", PanelStyle(new Color("d6bf83"), new Color("7d5a35")));
        button.Pressed += () =>
        {
            guidePopupId = id;
            RefreshTable();
        };
        return button;
    }

    private Control CreateGuidePopup(string titleText, string bodyText)
    {
        var panel = new Panel
        {
            Position = new Vector2(266, 166),
            Size = new Vector2(620, 316),
            ZIndex = 110
        };
        panel.AddThemeStyleboxOverride("panel", PanelStyle(new Color("2f2a24"), new Color("d6c07a")));
        panel.AddChild(CreateCloseButton(new Vector2(572, 12), () =>
        {
            guidePopupId = null;
            RefreshTable();
        }));

        var title = new Label
        {
            Position = new Vector2(34, 26),
            Size = new Vector2(520, 30),
            Text = titleText,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeColorOverride("font_color", new Color("f5e5b8"));
        title.AddThemeFontSizeOverride("font_size", 22);
        panel.AddChild(title);

        var body = new Label
        {
            Position = new Vector2(42, 78),
            Size = new Vector2(536, 200),
            Text = bodyText,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        body.AddThemeColorOverride("font_color", Colors.White);
        body.AddThemeFontSizeOverride("font_size", 18);
        panel.AddChild(body);
        return panel;
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

    private static Dictionary<string, (string Title, string Text)> ReadGuideSections()
    {
        var sections = new Dictionary<string, (string Title, string Text)>();
        using var file = Godot.FileAccess.Open(GameText.Path("table-guide.md"), Godot.FileAccess.ModeFlags.Read);
        if (file is null)
        {
            return sections;
        }

        string? id = null;
        var title = "";
        var lines = new List<string>();
        while (!file.EofReached())
        {
            var line = file.GetLine().Trim();
            if (line.StartsWith("## `"))
            {
                StoreSection();
                var end = line.IndexOf('`', 4);
                var dash = line.IndexOf('—');
                id = end > 4 ? line[4..end] : null;
                title = dash >= 0 ? line[(dash + 1)..].Trim() : "";
                lines.Clear();
            }
            else if (id is not null && line.Length > 0 && !line.StartsWith("<!--") && !line.StartsWith("#"))
            {
                lines.Add(line);
            }
        }

        StoreSection();
        return sections;

        void StoreSection()
        {
            if (id is not null)
            {
                sections[id] = (title, string.Join("\n\n", lines));
            }
        }
    }

    private Control CreateAdviceBubble()
    {
        var bubble = new Control
        {
            Position = new Vector2(18, 18),
            Size = new Vector2(178, 82),
            ZIndex = 50
        };

        var tail = new SpeechBubbleTail
        {
            Position = new Vector2(118, 52),
            Size = new Vector2(40, 28),
            MouseFilter = MouseFilterEnum.Ignore
        };
        bubble.AddChild(tail);

        var button = new Button
        {
            Size = new Vector2(178, 58),
            Text = GameText.Pick($"Спросить совет\n{adviceCost}%", $"Ask for advice\n{adviceCost}%"),
            Disabled = !CanUseAdvice(),
            FocusMode = FocusModeEnum.None
        };
        button.AddThemeStyleboxOverride("normal", SpeechBubbleStyle(new Color("f2e3bf")));
        button.AddThemeStyleboxOverride("hover", SpeechBubbleStyle(new Color("fff0c8")));
        button.AddThemeStyleboxOverride("pressed", SpeechBubbleStyle(new Color("dcc48f")));
        button.AddThemeStyleboxOverride("disabled", SpeechBubbleStyle(new Color(0.34f, 0.30f, 0.24f, 0.72f)));
        button.AddThemeColorOverride("font_color", new Color("3b2719"));
        button.AddThemeColorOverride("font_disabled_color", new Color("8f8064"));
        button.Pressed += AskForAdvice;
        bubble.AddChild(button);
        return bubble;
    }

    private Control CreateDiscardPileArea()
    {
        var area = new Control
        {
            Position = DiscardPilePosition,
            CustomMinimumSize = DiscardPileSize,
            Size = DiscardPileSize,
            ZIndex = 4
        };

        var first = Math.Max(0, discardedCards.Count - VisibleDiscardCards);
        var visibleCount = discardedCards.Count - first;
        for (var index = first; index < discardedCards.Count; index++)
        {
            area.AddChild(CreateDiscardCardImage(discardedCards[index], index - first, index, visibleCount));
        }

        return area;
    }

    private Button CreateDiscardCardImage(Card card, int visibleIndex, int absoluteIndex, int visibleCount)
    {
        var path = VisibleCardTexturePath(card);
        var position = DiscardCardPosition(visibleIndex, absoluteIndex, visibleCount);
        var target = new Rect2(DiscardPilePosition + position, DiscardCardSize);
        var button = new DelayedCardButton
        {
            RevealAt = CardRevealAt(target),
            Position = position,
            CustomMinimumSize = DiscardCardSize,
            Size = DiscardCardSize,
            Icon = GD.Load<Texture2D>(path),
            ExpandIcon = true,
            FocusMode = FocusModeEnum.None,
            RotationDegrees = DiscardCardRotation(absoluteIndex),
            PivotOffset = DiscardCardSize / 2f
        };
        button.AddThemeColorOverride("icon_normal_color", Colors.White);
        button.AddThemeColorOverride("icon_hover_color", Colors.White);
        button.AddThemeColorOverride("icon_pressed_color", Colors.White);
        button.AddThemeStyleboxOverride("normal", EmptyStyle());
        button.AddThemeStyleboxOverride("hover", EmptyStyle());
        button.AddThemeStyleboxOverride("pressed", EmptyStyle());
        button.AddThemeStyleboxOverride("focus", EmptyStyle());
        button.Pressed += () =>
        {
            viewedCardTexturePath = path;
            RefreshTable();
        };
        return button;
    }

    private static Vector2 DiscardCardPosition(int visibleIndex, int absoluteIndex, int visibleCount)
    {
        var pileWidth = DiscardCardSize.X + Math.Max(0, visibleCount - 1) * 22f;
        var left = (DiscardPileSize.X - pileWidth) / 2f;
        return new Vector2(
            left + visibleIndex * 22 + ((absoluteIndex * 13) % 11) - 5,
            20 + ((absoluteIndex * 17) % 30) - visibleIndex * 2);
    }

    private static float DiscardCardRotation(int absoluteIndex)
    {
        return ((absoluteIndex * 37) % 25) - 12;
    }

    private Control CreateCardFlightLayer()
    {
        var layer = new Control
        {
            Size = ScreenSize,
            MouseFilter = MouseFilterEnum.Ignore,
            ZIndex = 80
        };

        foreach (var flight in cardFlights)
        {
            layer.AddChild(new FlyingCard { Flight = flight });
        }

        return layer;
    }

    private bool CanPlaySelected()
    {
        return !IsGameOver() &&
            selectedHandIndex == 0 &&
            selectedCardIndex is not null;
    }

    private bool CanDropSelected()
    {
        return !IsGameOver() && selectedHandIndex == 0 && selectedCardIndex is not null;
    }

    private bool CanHintSelected()
    {
        return !IsGameOver() &&
            hintTokens > 0 &&
            selectedHandIndex == HintTargetHandIndex() &&
            selectedCardIndex is not null;
    }

    private bool CanUseAdvice()
    {
        return !IsGameOver() &&
            !partnerAdviceOverlayVisible &&
            hintTokens > 0 &&
            CanPayAdvice(0) &&
            game.Hands[0].Count > 0 &&
            partnerAdviceCardIndex is null;
    }

    private void PlaySelected()
    {
        if (!CanPlaySelected())
        {
            return;
        }

        var hand = game.Hands[0];
        var playedIndex = selectedCardIndex!.Value;
        var card = hand.Slots[playedIndex].Card;
        var acceptedAdvice = partnerAdviceCardIndex == playedIndex;
        var lane = LaneFor(card.Color);
        var playSucceeded = burnedFromRank[lane] is null && card.Rank == laneProgress[lane] + 1;
        if (playSucceeded)
        {
            QueuePlayFlight(card, handIndex: 0, playedIndex);
        }
        else
        {
            QueueDiscardFlight(card, handIndex: 0, playedIndex);
        }
        playerHandMessage = "";
        lastSelected = CardText(card);
        AddLog($"ACTION: Player {currentPlayerIndex + 1} played {CardText(card)}");
        if (acceptedAdvice)
        {
            AddLog("Accepted partner advice");
            ClearAdvice();
        }
        else
        {
            ClearIgnoredAdvice($"played {CardText(card)}");
        }

        hand.RemoveAt(selectedCardIndex.Value);

        if (playSucceeded)
        {
            laneProgress[lane] = card.Rank;
            lastAction = TableMessages.Played(partner: false, PlanLanes[lane], card.Rank);
            lastResult = TableMessages.PlayResult(PlanLanes[lane], card.Rank, success: true, gameOver: false);
            AddLog($"SUCCESS: {PlanLanes[lane]} progress {laneProgress[lane]}/5");
            if (card.Rank == 5)
            {
                RestoreHintToken();
            }
            QueueVictoryCutscene();
        }
        else
        {
            AddDiscard(card);
            mistakesLeft--;
            burnedFromRank[lane] ??= laneProgress[lane] + 1;
            RemoveColorFromHands(card.Color);
            QueueMistakeCutscene();
            lastAction = TableMessages.Played(partner: false, PlanLanes[lane], card.Rank);
            lastResult = TableMessages.PlayResult(PlanLanes[lane], burnedFromRank[lane]!.Value, success: false, IsGameOver());
            AddLog("FAILURE: wrong card");
            AddLog($"Burned lane: {PlanLanes[lane]}");
            AddLog($"Mistakes left: {mistakesLeft}");
        }

        RefillHand(hand);
        ClearSelection();
        AdvanceTurn();
        RefreshTable();
    }

    private void DropSelected()
    {
        if (!CanDropSelected())
        {
            return;
        }

        var hand = game.Hands[0];
        var droppedIndex = selectedCardIndex!.Value;
        var card = hand.Slots[droppedIndex].Card;
        QueueDiscardFlight(card, handIndex: 0, droppedIndex);
        playerHandMessage = "";
        lastSelected = CardText(card);
        AddLog($"ACTION: Player {currentPlayerIndex + 1} discarded {lastSelected}");
        RestoreHintToken();
        ClearIgnoredAdvice($"discarded {lastSelected}");
        AddDiscard(card);
        hand.RemoveAt(droppedIndex);
        RefillHand(hand);
        lastAction = TableMessages.PlayerDiscarded;
        lastResult = TableMessages.PlayerHandRefilled;
        AddLog("Hand refilled");
        ClearSelection();
        AdvanceTurn();
        RefreshTable();
    }

    private void HintSelectedColor()
    {
        if (!CanHintSelected())
        {
            return;
        }

        var targetHandIndex = HintTargetHandIndex();
        var card = game.Hands[targetHandIndex].Slots[selectedCardIndex!.Value].Card;
        hintTokens--;
        playerHandMessage = "";
        ApplyShareChange(currentPlayerIndex);
        ApplyVisualHint(handIndex: targetHandIndex, color: card.Color, rank: null);
        lastSelected = CardText(card);
        lastAction = TableMessages.Hint(partner: false, card.Color, rank: null);
        lastResult = TableMessages.CardsMarked;
        AddLog($"ACTION: Player {currentPlayerIndex + 1} gave color hint: {card.Color}");
        AddLog($"Marked cards: {MarkedCardsText(targetHandIndex)}");
        ClearIgnoredAdvice($"gave color hint {card.Color}");
        ClearSelection();
        AdvanceTurn();
        RefreshTable();
    }

    private void HintSelectedRank()
    {
        if (!CanHintSelected())
        {
            return;
        }

        var targetHandIndex = HintTargetHandIndex();
        var card = game.Hands[targetHandIndex].Slots[selectedCardIndex!.Value].Card;
        hintTokens--;
        playerHandMessage = "";
        ApplyShareChange(currentPlayerIndex);
        ApplyVisualHint(handIndex: targetHandIndex, color: null, rank: card.Rank);
        lastSelected = CardText(card);
        lastAction = TableMessages.Hint(partner: false, color: null, card.Rank);
        lastResult = TableMessages.CardsMarked;
        AddLog($"ACTION: Player {currentPlayerIndex + 1} gave rank hint: {card.Rank}");
        AddLog($"Marked cards: {MarkedCardsText(targetHandIndex)}");
        ClearIgnoredAdvice($"gave rank hint {card.Rank}");
        ClearSelection();
        AdvanceTurn();
        RefreshTable();
    }

    private void AskForAdvice()
    {
        if (!CanUseAdvice())
        {
            return;
        }

        if (!TryFindPlayableCard(0, out var adviceIndex))
        {
            playerHandMessage = TableMessages.NoAdvice;
            lastSelected = GameText.Pick("Партнёр", "Partner");
            lastAction = TableMessages.AdviceRefused;
            lastResult = TableMessages.NoGoodMove;
            AddLog("Advice requested:");
            AddLog("Partner refused to suggest a card");
            ClearSelection();
            RefreshTable();
            return;
        }

        hintTokens--;
        var cost = PayForAdvice(1);
        playerHandMessage = "";
        partnerAdviceCardIndex = adviceIndex;
        partnerAdviceCard = game.Hands[0].Slots[partnerAdviceCardIndex.Value].Card;
        lastSelected = GameText.Pick(
            $"Партнёр выбрал карту {partnerAdviceCardIndex.Value + 1}",
            $"Your partner picked card {partnerAdviceCardIndex.Value + 1}");
        lastAction = TableMessages.AdviceGiven(cost);
        lastResult = TableMessages.PartnerSuggestedCard;
        AddLog("Advice requested:");
        AddLog($"Suggested card: {CardText(partnerAdviceCard)}");
        AddLog("Cost: 1 hint token");
        AddLog($"Cost: {cost}% share");
        ClearSelection();
        RefreshTable();
    }

    private void ExportLog()
    {
        SaveLog();
        lastAction = TableMessages.LogsGathered;
        lastResult = TableMessages.LogSaved(LogPath);
        RefreshTable();
    }

    private void CashOut()
    {
        if (Score() < 16 || mistakesLeft <= 0)
        {
            return;
        }

        lastAction = GameText.Pick("Забрали добычу.", "We took the money and ran.");
        lastResult = GameText.Pick($"План готов на {Score()}/25.", $"The plan reached {Score()}/25.");
        goodGameOverCutscenePending = true;
        RefreshTable();
    }

    private void ConfirmCashOut()
    {
        var dialog = new ConfirmationDialog
        {
            Title = GameText.Pick("На том и порешим?", "Call it a deal?"),
            DialogText = GameText.Pick(
                $"Твоя доля: {playerShare}% ({TableMessages.LootAmount(playerShare)}).\n{TableMessages.LootReply(playerShare)}",
                $"Your cut: {playerShare}% ({TableMessages.LootAmount(playerShare)}).\n{TableMessages.LootReply(playerShare)}"),
            OkButtonText = GameText.Pick("На том и порешим", "Take it and run"),
            CancelButtonText = GameText.Pick("Продолжить", "Keep going"),
            Exclusive = true
        };
        dialog.Confirmed += CashOut;
        dialog.CloseRequested += dialog.QueueFree;
        dialog.GetCancelButton().Pressed += dialog.QueueFree;
        AddChild(dialog);
        dialog.PopupCentered(new Vector2I(560, 220));
    }

    private void RefillHand(Hand hand)
    {
        while (hand.Count < 5 && TryDrawPlayableCard(out var card))
        {
            QueueDrawFlight(card, HandIndex(hand), hand.Count);
            hand.Add(card);
        }
    }

    private int HandIndex(Hand hand)
    {
        return ReferenceEquals(hand, game.Hands[0]) ? 0 : 1;
    }

    private void QueueDrawFlight(Card card, int handIndex, int cardIndex)
    {
        var target = HandCardFlightRect(handIndex, cardIndex);
        var texturePath = handIndex == 0 ? CardBackTexturePath : VisibleCardTexturePath(card);
        var start = CenteredRect(DeckFlightRect(), CardSize);
        var rotation = HandCardRotation(handIndex, cardIndex);
        QueueCardFlight(start, target, texturePath, texturePath, rotation, rotation, flip: false);
    }

    private void QueuePlayFlight(Card card, int handIndex, int cardIndex)
    {
        var start = CardSceneRect(handIndex, cardIndex, HandCardFlightRect(handIndex, cardIndex));
        var target = PlanCardFlightRect(card);
        var faceTexturePath = VisibleCardTexturePath(card);
        QueueCardFlight(
            start,
            target,
            handIndex == 0 ? CardBackTexturePath : faceTexturePath,
            faceTexturePath,
            HandCardRotation(handIndex, cardIndex),
            PlayedCardRotation(card.Color, card.Rank),
            flip: handIndex == 0);
    }

    private void QueueDiscardFlight(Card card, int handIndex, int cardIndex)
    {
        var start = CardSceneRect(handIndex, cardIndex, HandCardFlightRect(handIndex, cardIndex));
        var faceTexturePath = VisibleCardTexturePath(card);
        var target = DiscardCardFlightRect();
        QueueCardFlight(
            start,
            target,
            handIndex == 0 ? CardBackTexturePath : faceTexturePath,
            faceTexturePath,
            HandCardRotation(handIndex, cardIndex),
            DiscardCardRotation(discardedCards.Count),
            flip: handIndex == 0);
    }

    private void QueueCardFlight(
        Rect2 start,
        Rect2 target,
        string startTexturePath,
        string targetTexturePath,
        float startRotation,
        float targetRotation,
        bool flip)
    {
        cardFlights.Add(new CardFlight(
            startTexturePath,
            targetTexturePath,
            start.Position,
            target.Position,
            start.Size,
            target.Size,
            startRotation,
            targetRotation,
            flip,
            Time.GetTicksMsec() / 1000.0));
    }

    private Rect2 CardSceneRect(int handIndex, int cardIndex, Rect2 fallback)
    {
        if (FindChild(CardNodeName(handIndex, cardIndex), recursive: true, owned: false) is Control card)
        {
            var size = card.Size;
            return new Rect2(card.GlobalPosition, size.X > 0 && size.Y > 0 ? size : fallback.Size);
        }

        return fallback;
    }

    private static Rect2 HandCardFlightRect(int handIndex, int index)
    {
        return handIndex == 0 ? PlayerCardFlightRect(index) : PartnerCardFlightRect(index);
    }

    private static Rect2 PlayerCardFlightRect(int index)
    {
        var layoutWidth = ScreenSize.X - LayoutPadding.X * 2;
        var fanPosition = LayoutPadding + new Vector2(
            (layoutWidth - PlayerHandFanSize.X) / 2f,
            PartnerAreaHeight + LayoutSeparation + CenterRowHeight + LayoutSeparation);
        var cardPosition = fanPosition + PlayerHandFanStart + new Vector2(HandTableOffset + index * 84, Math.Abs(index - 2) * 10);
        return new Rect2(cardPosition, PlayerCardSize);
    }

    private static Rect2 PartnerCardFlightRect(int index)
    {
        const int handSpacing = 8;
        var layoutWidth = ScreenSize.X - LayoutPadding.X * 2;
        var handWidth = PartnerCardSize.X * 5 + handSpacing * 4;
        var cardPosition = LayoutPadding + new Vector2(
            HandTableOffset + (layoutWidth - handWidth) / 2f + index * (PartnerCardSize.X + handSpacing),
            28);
        return new Rect2(cardPosition, PartnerCardSize);
    }

    private static Rect2 DeckFlightRect()
    {
        var centerRowPosition = LayoutPadding + new Vector2(0, PartnerAreaHeight + LayoutSeparation);
        return new Rect2(centerRowPosition + StockAreaPosition, StockDeckSize);
    }

    private static Rect2 PlanCardFlightRect(Card card)
    {
        var lane = LaneFor(card.Color);
        var lanesWidth = PlanLaneSize.X * PlanLanes.Length + PlanLaneSpacing * (PlanLanes.Length - 1);
        var lanePosition = new Vector2(
            (PlanAreaWidth - lanesWidth) / 2f + lane * (PlanLaneSize.X + PlanLaneSpacing),
            0);
        var centerRowPosition = LayoutPadding + new Vector2(0, PartnerAreaHeight + LayoutSeparation);
        return new Rect2(centerRowPosition + PlanAreaPosition + lanePosition + new Vector2(20, 24 + (card.Rank - 1) * 22), PlayedCardSize);
    }

    private Rect2 DiscardCardFlightRect()
    {
        var visibleCount = Math.Min(discardedCards.Count + 1, VisibleDiscardCards);
        var position = DiscardCardPosition(visibleCount - 1, discardedCards.Count, visibleCount);
        return new Rect2(DiscardPilePosition + position, DiscardCardSize);
    }

    private double CardRevealAt(Rect2 target)
    {
        var flight = cardFlights.LastOrDefault(candidate =>
            !candidate.IsDone &&
            candidate.Target.DistanceTo(target.Position) < 0.5f &&
            candidate.TargetSize.DistanceTo(target.Size) < 0.5f);
        return flight is null ? 0 : flight.StartedAt + CardFlightDuration * 0.86;
    }

    private static Rect2 CenteredRect(Rect2 source, Vector2 size)
    {
        return new Rect2(source.Position + (source.Size - size) / 2f, size);
    }

    private static float HandCardRotation(int handIndex, int index)
    {
        return handIndex == 0 ? PlayerHandRotation(index) : 0f;
    }

    private static float PlayerHandRotation(int index)
    {
        return (index - 2) * 7f;
    }

    private static string CardNodeName(int handIndex, int cardIndex)
    {
        return $"Card_{handIndex}_{cardIndex}";
    }

    private static string VisibleCardTexturePath(Card card)
    {
        var path = CardTexturePath(card.Color, card.Rank);
        return ResourceLoader.Exists(path) ? path : CardBackTexturePath;
    }

    private bool TryDrawPlayableCard(out Card card)
    {
        while (!game.DrawDeck.IsEmpty)
        {
            card = game.DrawDeck.Draw();
            if (burnedFromRank[LaneFor(card.Color)] is null)
            {
                return true;
            }
        }

        card = new Card(CardColor.Red, 1);
        return false;
    }

    private void RemoveColorFromHands(CardColor color)
    {
        foreach (var hand in game.Hands)
        {
            for (var i = hand.Count - 1; i >= 0; i--)
            {
                if (hand.Slots[i].Card.Color == color)
                {
                    hand.RemoveAt(i);
                }
            }

            RefillHand(hand);
        }
    }

    private void SelectCard(int handIndex, int cardIndex)
    {
        var alreadySelected = selectedHandIndex == handIndex && selectedCardIndex == cardIndex;
        selectedHandIndex = handIndex;
        selectedCardIndex = cardIndex;
        cardOverlayVisible = true;
        lastSelected = SelectedText();
        lastAction = TableMessages.CardSelected;
        lastResult = TableMessages.AwaitingAction;
        var card = game.Hands[handIndex].Slots[cardIndex].Card;
        if (!alreadySelected)
        {
            AddLog($"CARD SELECT: Player {currentPlayerIndex + 1} selected card {CardText(card)}");
        }

        RefreshTable();
    }

    private void ClearSelection()
    {
        selectedHandIndex = null;
        selectedCardIndex = null;
        cardOverlayVisible = false;
    }

    private void ApplyVisualHint(int handIndex, CardColor? color, int? rank)
    {
        hintedHandIndex = handIndex;
        hintedColor = color;
        hintedRank = rank;
    }

    private void ApplyShareChange(int earningPlayerIndex, int percent = 5)
    {
        if (earningPlayerIndex == 0)
        {
            playerShare = Math.Min(100, playerShare + percent);
            partnerShare = Math.Max(0, partnerShare - percent);
        }
        else
        {
            partnerShare = Math.Min(100, partnerShare + percent);
            playerShare = Math.Max(0, playerShare - percent);
        }
    }

    private bool CanPayAdvice(int payerPlayerIndex)
    {
        return (payerPlayerIndex == 0 ? playerShare : partnerShare) >= adviceCost;
    }

    private int PayForAdvice(int earningPlayerIndex)
    {
        var cost = adviceCost;
        ApplyShareChange(earningPlayerIndex, cost);
        adviceCost += AdviceCostStep;
        return cost;
    }

    private int HintTargetHandIndex()
    {
        return 1 - currentPlayerIndex;
    }

    private void ClearAdvice()
    {
        partnerAdviceCardIndex = null;
        partnerAdviceCard = null;
    }

    private void AddDiscard(Card card)
    {
        discardedCards.Add(card);
    }

    private void ClearIgnoredAdvice(string actualAction)
    {
        if (partnerAdviceCard is null)
        {
            return;
        }

        AddLog("Advice ignored");
        AddLog($"Suggested card: {CardText(partnerAdviceCard)}");
        AddLog($"Actual action: {actualAction}");
        ClearAdvice();
    }

    private void AdvanceTurn()
    {
        turnNumber++;
        currentPlayerIndex = 1 - currentPlayerIndex;

        if (currentPlayerIndex == 1 && !IsGameOver())
        {
            if (CanPartnerAskAdvice())
            {
                StartPartnerAdviceRequest();
                return;
            }

            RunPartnerTurn();
            FinishPartnerTurn();
        }
    }

    private bool CanPartnerAskAdvice()
    {
        return hintTokens > 0 && CanPayAdvice(1) && game.Hands[1].Count > 0;
    }

    private void StartPartnerAdviceRequest()
    {
        partnerAdviceOverlayVisible = true;
        playerHandMessage = "";
        lastSelected = GameText.Pick("Партнёр просит совет", "Your partner wants advice");
        lastAction = TableMessages.PartnerAsksAdvice(adviceCost);
        lastResult = TableMessages.ChoosePartnerCard;
        AddLog("Partner asked for advice");
        AddLog($"Advice price: {adviceCost}% share");
        ClearSelection();
    }

    private void FinishPartnerTurn()
    {
        if (!IsGameOver())
        {
            turnNumber++;
            currentPlayerIndex = 0;
        }
    }

    private void RunPartnerTurn(bool allowHint = true)
    {
        if (TryFindPlayableCard(1, out var playableIndex))
        {
            AddLog("Partner decision:");
            AddLog("Reason: playable card available");
            PartnerPlay(playableIndex);
            return;
        }

        if (allowHint && hintTokens > 0 && game.Hands[0].Count > 0)
        {
            AddLog("Partner decision:");
            AddLog("Reason: no playable cards, giving hint");
            PartnerHint();
            return;
        }

        AddLog("Partner decision:");
        AddLog("Reason: no useful action, discarding");
        PartnerDiscard();
    }

    private bool TryFindPlayableCard(int handIndex, out int index)
    {
        var hand = game.Hands[handIndex];
        for (var i = 0; i < hand.Count; i++)
        {
            var card = hand.Slots[i].Card;
            var lane = LaneFor(card.Color);
            if (burnedFromRank[lane] is null && card.Rank == laneProgress[lane] + 1)
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }

    private void PartnerPlay(int index)
    {
        var hand = game.Hands[1];
        if (index < 0 || index >= hand.Count)
        {
            return;
        }

        var card = hand.Slots[index].Card;
        AddLog($"ACTION: Player 2 played {CardText(card)}");
        var lane = LaneFor(card.Color);
        var playSucceeded = burnedFromRank[lane] is null && card.Rank == laneProgress[lane] + 1;
        if (playSucceeded)
        {
            QueuePlayFlight(card, handIndex: 1, index);
        }
        else
        {
            QueueDiscardFlight(card, handIndex: 1, index);
        }

        hand.RemoveAt(index);
        if (playSucceeded)
        {
            laneProgress[lane] = card.Rank;
            lastAction = TableMessages.Played(partner: true, PlanLanes[lane], card.Rank);
            lastResult = TableMessages.PlayResult(PlanLanes[lane], card.Rank, success: true, gameOver: false);
            AddLog($"SUCCESS: {PlanLanes[lane]} progress {laneProgress[lane]}/5");
            if (card.Rank == 5)
            {
                RestoreHintToken();
            }
            QueueVictoryCutscene();
        }
        else
        {
            AddDiscard(card);
            mistakesLeft--;
            burnedFromRank[lane] ??= laneProgress[lane] + 1;
            RemoveColorFromHands(card.Color);
            QueueMistakeCutscene();
            lastAction = TableMessages.Played(partner: true, PlanLanes[lane], card.Rank);
            lastResult = TableMessages.PlayResult(PlanLanes[lane], burnedFromRank[lane]!.Value, success: false, IsGameOver());
            AddLog("FAILURE: wrong card");
            AddLog($"Burned lane: {PlanLanes[lane]}");
            AddLog($"Mistakes left: {mistakesLeft}");
        }

        RefillHand(hand);
    }

    private void PartnerDiscard()
    {
        var hand = game.Hands[1];
        if (hand.Count == 0)
        {
            AddLog("Partner skipped: no cards");
            return;
        }

        var index = random.Next(hand.Count);
        var card = hand.Slots[index].Card;
        QueueDiscardFlight(card, handIndex: 1, index);
        AddDiscard(card);
        hand.RemoveAt(index);
        lastAction = TableMessages.PartnerDiscarded;
        lastResult = TableMessages.PartnerHandRefilled;
        AddLog($"ACTION: Player 2 discarded {CardText(card)}");
        RestoreHintToken();
        RefillHand(hand);
        AddLog("Hand refilled");
    }

    private void RestoreHintToken()
    {
        if (hintTokens < MaxHintTokens)
        {
            hintTokens++;
            AddLog("Hint token restored");
        }
    }

    private void PartnerHint()
    {
        if (!TryFindPlayableCard(0, out var hintIndex))
        {
            PartnerRefuseHint();
            return;
        }

        var card = game.Hands[0].Slots[hintIndex].Card;
        var givesColor = random.Next(2) == 0;
        hintTokens--;
        playerHandMessage = "";
        ApplyShareChange(1);
        ApplyVisualHint(0, givesColor ? card.Color : null, givesColor ? null : card.Rank);
        lastAction = TableMessages.Hint(partner: true, givesColor ? card.Color : null, givesColor ? null : card.Rank);
        lastResult = TableMessages.CardsMarked;
        AddLog(givesColor
            ? $"ACTION: Player 2 gave color hint: {card.Color}"
            : $"ACTION: Player 2 gave rank hint: {card.Rank}");
        AddLog($"Marked cards: {MarkedCardsText(0)}");
    }

    private void PartnerRefuseHint()
    {
        AddLog("Partner refused to give a hint");
        PartnerDiscard();
        playerHandMessage = TableMessages.NoAdvice;
        lastAction = TableMessages.PartnerRefusedHint;
        lastResult = TableMessages.PartnerDiscardedInstead;
    }

    private void ResolvePartnerPlayAdvice(int index)
    {
        if (!partnerAdviceOverlayVisible || IsGameOver() || hintTokens <= 0 || !CanPayAdvice(1))
        {
            return;
        }

        partnerAdviceOverlayVisible = false;
        playerHandMessage = "";
        hintTokens--;
        var cost = PayForAdvice(0);
        AddLog("Player advice to partner:");
        AddLog($"Suggested play: card {index + 1}");
        PartnerPlay(index);
        AddLog("Accepted player advice");
        AddLog("Cost: 1 hint token");
        AddLog($"Cost: {cost}% share");
        ClearSelection();
        FinishPartnerTurn();
        RefreshTable();
    }

    private void ResolvePartnerNoPlayAdvice()
    {
        if (!partnerAdviceOverlayVisible || IsGameOver() || hintTokens <= 0 || !CanPayAdvice(1))
        {
            return;
        }

        partnerAdviceOverlayVisible = false;
        playerHandMessage = "";
        hintTokens--;
        var cost = PayForAdvice(0);
        AddLog("Player advice to partner:");
        AddLog("Suggested action: don't play");
        if (hintTokens > 0 && game.Hands[0].Count > 0)
        {
            PartnerHint();
        }
        else
        {
            PartnerDiscard();
        }

        AddLog("Accepted player advice");
        AddLog("Cost: 1 hint token");
        AddLog($"Cost: {cost}% share");
        ClearSelection();
        FinishPartnerTurn();
        RefreshTable();
    }

    private void ResolvePartnerNoAdvice()
    {
        if (!partnerAdviceOverlayVisible || IsGameOver())
        {
            return;
        }

        partnerAdviceOverlayVisible = false;
        playerHandMessage = "";
        AddLog("Player advice to partner:");
        AddLog("Suggested action: no advice");
        RunPartnerTurn(allowHint: false);
        ClearSelection();
        FinishPartnerTurn();
        RefreshTable();
    }

    private Control CreateViewedCardOverlay()
    {
        var overlay = new ColorRect
        {
            Size = ScreenSize,
            Color = new Color(0, 0, 0, 0.62f),
            MouseFilter = MouseFilterEnum.Stop,
            ZIndex = 100
        };
        overlay.GuiInput += @event =>
        {
            if (@event is InputEventMouseButton { Pressed: true })
            {
                viewedCardTexturePath = null;
                RefreshTable();
            }
        };

        var cardPosition = (ScreenSize - OverlayCardSize) / 2f;
        overlay.AddChild(new TextureRect
        {
            Position = cardPosition,
            CustomMinimumSize = OverlayCardSize,
            Size = OverlayCardSize,
            Texture = GD.Load<Texture2D>(viewedCardTexturePath!),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Stop
        });
        return overlay;
    }

    private bool IsGameOver()
    {
        return mistakesLeft <= 0 || IsGoodEnding();
    }

    private bool IsGoodEnding()
    {
        return laneProgress.Count(rank => rank >= 5) >= 3 &&
            Enumerable.Range(0, PlanLanes.Length).All(lane => laneProgress[lane] >= 5 || burnedFromRank[lane] is not null);
    }

    private int Score()
    {
        return laneProgress.Sum();
    }

    private void QueueVictoryCutscene()
    {
        goodGameOverCutscenePending |= IsGoodEnding();
    }

    private void QueueMistakeCutscene()
    {
        if (mistakesLeft == MaxMistakes - 1)
        {
            mistakeOneCutscenePending = true;
        }
        else if (mistakesLeft == MaxMistakes - 2)
        {
            mistakeTwoCutscenePending = true;
        }
        else if (mistakesLeft <= 0)
        {
            badGameOverCutscenePending = true;
        }

        if (mistakesLeft > 0)
        {
            QueueVictoryCutscene();
        }
    }

    private void StartPendingMistakeCutscene()
    {
        if (cutscenePlaying)
        {
            return;
        }

        Control? cutscene = null;
        Vector2? zoomTarget = null;
        if (mistakeOneCutscenePending)
        {
            mistakeOneCutscenePending = false;
            cutscene = new MistakeOneCutscene { Finished = FinishMistakeCutscene };
            zoomTarget = FirstMistakeZoomTarget;
        }
        else if (mistakeTwoCutscenePending)
        {
            mistakeTwoCutscenePending = false;
            cutscene = new MistakeTwoCutscene { Finished = FinishMistakeCutscene };
            zoomTarget = SecondMistakeZoomTarget;
        }
        else if (badGameOverCutscenePending)
        {
            badGameOverCutscenePending = false;
            cutscene = new BadGameOverCutscene();
            zoomTarget = LastMistakeZoomTarget;
        }
        else if (goodGameOverCutscenePending)
        {
            goodGameOverCutscenePending = false;
            cutscene = new GoodGameOverCutscene { PlayerShare = playerShare };
        }

        if (cutscene is null)
        {
            return;
        }

        cutscenePlaying = true;
        if (zoomTarget is Vector2 target)
        {
            _ = StartCutsceneAfterZoom(cutscene, target);
        }
        else
        {
            GetParent().AddChild(cutscene);
        }
    }

    private async System.Threading.Tasks.Task StartCutsceneAfterZoom(Control cutscene, Vector2 target)
    {
        const float zoom = 1.45f;
        var tween = CreateTween().SetParallel();
        tween.TweenProperty(this, "scale", Vector2.One * zoom, 1.1).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(this, "position", ScreenSize / 2f - target * zoom, 1.1).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);

        await ToSignal(GetTree().CreateTimer(0.72), SceneTreeTimer.SignalName.Timeout);
        GetParent().AddChild(cutscene);
        await ToSignal(GetTree().CreateTimer(1.2), SceneTreeTimer.SignalName.Timeout);
        Scale = Vector2.One;
        Position = Vector2.Zero;
    }

    private void FinishMistakeCutscene()
    {
        cutscenePlaying = false;
        StartPendingMistakeCutscene();
    }

    private string SelectedText()
    {
        if (selectedHandIndex is null || selectedCardIndex is null)
        {
            return lastSelected;
        }

        var hand = game.Hands[selectedHandIndex.Value];
        if (selectedCardIndex.Value < 0 || selectedCardIndex.Value >= hand.Count)
        {
            return lastSelected;
        }

        var card = hand.Slots[selectedCardIndex.Value].Card;
        return selectedHandIndex == 0
            ? GameText.Pick(
                $"Игрок: карта {selectedCardIndex.Value + 1} ({CardText(card)})",
                $"You: card {selectedCardIndex.Value + 1} ({CardText(card)})")
            : GameText.Pick($"Партнёр: {CardText(card)}", $"Partner: {CardText(card)}");
    }

    private void AddLog(string entry)
    {
        actionLog.Add($"Turn {turnNumber}:");
        actionLog.Add(entry);
        actionLog.Add("");
        SaveLog();
    }

    private void SaveLog()
    {
        File.WriteAllText(LogPath, string.Join('\n', actionLog));
    }

    private string MarkedCardsText(int handIndex)
    {
        var marked = game.Hands[handIndex].Slots
            .Select((slot, index) => IsHintedCard(slot.Card, handIndex) ? $"{index + 1}:{CardText(slot.Card)}" : "")
            .Where(text => text.Length > 0);
        return string.Join(", ", marked.DefaultIfEmpty(GameText.Pick("нет", "none")));
    }

    private static VBoxContainer CreateZone(string name, Vector2 minimumSize, bool expand = true)
    {
        return new VBoxContainer
        {
            Name = name,
            Alignment = BoxContainer.AlignmentMode.Center,
            CustomMinimumSize = minimumSize,
            SizeFlagsHorizontal = expand ? SizeFlags.ExpandFill : SizeFlags.ShrinkCenter
        };
    }

    private static void AddTitle(VBoxContainer layout, string text, Color? color = null)
    {
        var stage = new Control
        {
            CustomMinimumSize = new Vector2(1, 24),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        var label = new Label
        {
            Position = new Vector2(HandTableOffset, 0),
            Size = new Vector2(ScreenSize.X - LayoutPadding.X * 2, 24),
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        label.AddThemeFontSizeOverride("font_size", 18);
        label.AddThemeColorOverride("font_color", color ?? Colors.White);
        stage.AddChild(label);
        layout.AddChild(stage);
    }

    private void AddHand(VBoxContainer layout, Hand hand, int handIndex, bool hideCards)
    {
        if (hideCards && handIndex == 0)
        {
            layout.AddChild(CreatePlayerHandFan(hand));
            return;
        }

        var cardSize = handIndex == 1 ? PartnerCardSize : CardSize;
        var stage = new Control
        {
            CustomMinimumSize = new Vector2(1, cardSize.Y),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };

        var row = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            Position = new Vector2(HandTableOffset, 0),
            CustomMinimumSize = new Vector2(ScreenSize.X - LayoutPadding.X * 2, cardSize.Y),
            Size = new Vector2(ScreenSize.X - LayoutPadding.X * 2, cardSize.Y)
        };
        row.AddThemeConstantOverride("separation", 8);

        for (var index = 0; index < hand.Slots.Count; index++)
        {
            Vector2? size = handIndex == 1 ? PartnerCardSize : null;
            row.AddChild(CreateCard(hand.Slots[index].Card, handIndex, index, hideCards, size));
        }

        stage.AddChild(row);
        layout.AddChild(stage);
    }

    private Control CreatePlayerHandFan(Hand hand)
    {
        var fan = new Control
        {
            CustomMinimumSize = PlayerHandFanSize,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            ZIndex = 30
        };

        for (var index = 0; index < hand.Slots.Count; index++)
        {
            var button = CreateCard(hand.Slots[index].Card, 0, index, hidden: true, PlayerCardSize);
            var adviceLift = partnerAdviceCardIndex == index ? -24 : 0;
            button.Position = PlayerHandFanStart + new Vector2(HandTableOffset + index * 84, Math.Abs(index - 2) * 10 + adviceLift);
            button.RotationDegrees = PlayerHandRotation(index);
            button.PivotOffset = PlayerCardSize / 2;
            fan.AddChild(button);
        }

        return fan;
    }

    private Button CreateCard(Card card, int handIndex, int cardIndex, bool hidden, Vector2? size = null, Action? pressed = null)
    {
        var cardSize = size ?? CardSize;
        var selected = selectedHandIndex == handIndex && selectedCardIndex == cardIndex;
        var advised = handIndex == 0 && partnerAdviceCardIndex == cardIndex;
        var blocked = burnedFromRank[LaneFor(card.Color)] is not null;
        var showsHint = handIndex == 0;
        var hintedByColor = showsHint && hintedHandIndex == handIndex && hintedColor == card.Color;
        var hintedByRank = showsHint && hintedHandIndex == handIndex && hintedRank == card.Rank;
        var hinted = hintedByColor || hintedByRank;
        var faceTexturePath = CardTexturePath(card.Color, card.Rank);
        var hasFaceTexture = !hidden && ResourceLoader.Exists(faceTexturePath);
        var imageCard = hidden || hasFaceTexture;
        var text = hidden
            ? ""
            : hasFaceTexture ? ""
            : hinted ? $"{WesternSymbol(card.Color)}\n{card.Rank}\n{HintMarker()}" : $"{WesternSymbol(card.Color)}\n{card.Rank}";
        var button = new Button
        {
            Name = CardNodeName(handIndex, cardIndex),
            CustomMinimumSize = cardSize,
            Size = cardSize,
            Text = text,
            Disabled = blocked || (IsGameOver() && !imageCard),
            FocusMode = FocusModeEnum.None
        };
        if (hidden)
        {
            button.Icon = GD.Load<Texture2D>(CardBackTexturePath);
            button.ExpandIcon = true;
        }
        else if (hasFaceTexture)
        {
            button.Icon = GD.Load<Texture2D>(faceTexturePath);
            button.ExpandIcon = true;
        }
        button.AddThemeFontSizeOverride("font_size", hinted ? 22 : 26);
        if (imageCard)
        {
            button.AddThemeColorOverride("icon_normal_color", Colors.White);
            button.AddThemeColorOverride("icon_hover_color", Colors.White);
            button.AddThemeColorOverride("icon_pressed_color", Colors.White);
            button.AddThemeColorOverride("icon_disabled_color", Colors.White);
            button.AddThemeStyleboxOverride("normal", EmptyStyle());
            button.AddThemeStyleboxOverride("hover", EmptyStyle());
            button.AddThemeStyleboxOverride("pressed", EmptyStyle());
            button.AddThemeStyleboxOverride("disabled", EmptyStyle());
            button.AddThemeStyleboxOverride("focus", EmptyStyle());
            if (hintedByColor)
            {
                button.AddChild(CreateColorHintBadge(card.Color, cardSize));
            }

            if (hintedByRank)
            {
                button.AddChild(CreateRankHintBadge(card.Rank, cardSize));
            }

            button.Pressed += pressed ?? (() => SelectCard(handIndex, cardIndex));
            return button;
        }

        button.AddThemeStyleboxOverride(
            "normal",
            PanelStyle(blocked ? new Color("382a2a") : CardTint(card.Color), hinted || selected || advised ? new Color("f0d35f") : Colors.White));
        button.AddThemeStyleboxOverride("pressed", PanelStyle(CardTint(card.Color), new Color("f0d35f")));
        button.Pressed += pressed ?? (() => SelectCard(handIndex, cardIndex));
        return button;
    }

    private Control CreatePartnerAdviceOverlay()
    {
        var overlay = new ColorRect
        {
            Size = ScreenSize,
            Color = new Color(0, 0, 0, 0.64f),
            MouseFilter = MouseFilterEnum.Stop,
            ZIndex = 100
        };

        var panel = new Panel
        {
            Position = new Vector2(246, 108),
            Size = new Vector2(660, 330)
        };
        panel.AddThemeStyleboxOverride("panel", PanelStyle(new Color("2f2a24"), new Color("d6c07a")));
        overlay.AddChild(panel);

        var title = new Label
        {
            Position = new Vector2(0, 18),
            Size = new Vector2(660, 28),
            Text = GameText.Pick($"Партнёр просит совет ({adviceCost}%)", $"Your partner wants advice ({adviceCost}%)"),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeColorOverride("font_color", Colors.White);
        title.AddThemeFontSizeOverride("font_size", 20);
        panel.AddChild(title);

        var hand = new HBoxContainer
        {
            Position = new Vector2(34, 66),
            Size = new Vector2(592, 158),
            Alignment = BoxContainer.AlignmentMode.Center
        };
        hand.AddThemeConstantOverride("separation", 10);
        panel.AddChild(hand);

        for (var index = 0; index < game.Hands[1].Count; index++)
        {
            var adviceIndex = index;
            hand.AddChild(CreateCard(
                game.Hands[1].Slots[index].Card,
                handIndex: 1,
                cardIndex: index,
                hidden: false,
                PartnerAdviceCardSize,
                () => ResolvePartnerPlayAdvice(adviceIndex)));
        }

        var waitButton = CreateActionButton(GameText.Pick("Не ходи", "Hold"), true, ResolvePartnerNoPlayAdvice);
        waitButton.Position = new Vector2(160, 254);
        waitButton.Size = new Vector2(150, 40);
        panel.AddChild(waitButton);

        var noAdviceButton = CreateActionButton(GameText.Pick("Не подскажу ничего", "I got nothing"), true, ResolvePartnerNoAdvice);
        noAdviceButton.Position = new Vector2(326, 254);
        noAdviceButton.Size = new Vector2(190, 40);
        panel.AddChild(noAdviceButton);
        return overlay;
    }

    private Control CreateCardOverlay()
    {
        var overlay = new ColorRect
        {
            Size = ScreenSize,
            Color = new Color(0, 0, 0, 0.58f),
            MouseFilter = MouseFilterEnum.Stop,
            ZIndex = 100
        };
        overlay.GuiInput += @event =>
        {
            if (@event is InputEventMouseButton { Pressed: true })
            {
                CloseCardOverlay();
            }
        };

        var cardPosition = (ScreenSize - OverlayCardSize) / 2;
        var texturePath = SelectedCardTexturePath();
        if (texturePath is not null)
        {
            overlay.AddChild(new TextureRect
            {
                Position = cardPosition,
                CustomMinimumSize = OverlayCardSize,
                Size = OverlayCardSize,
                Texture = GD.Load<Texture2D>(texturePath),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = MouseFilterEnum.Ignore
            });
        }
        overlay.AddChild(CreateChoiceWheel(cardPosition));

        return overlay;
    }

    private void CloseCardOverlay()
    {
        ClearSelection();
        RefreshTable();
    }

    private Control CreateChoiceWheel(Vector2 cardPosition)
    {
        var wheel = new Control
        {
            Position = cardPosition + new Vector2(OverlayCardSize.X + 26, 96),
            Size = new Vector2(360, 208),
            MouseFilter = MouseFilterEnum.Stop
        };

        var drum = new RevolverDrum
        {
            Size = new Vector2(180, 180),
            Texture = GD.Load<Texture2D>(RevolverDrumTexturePath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            PivotOffset = new Vector2(90, 90),
            Rotation = Mathf.Pi / 2f,
            MouseFilter = MouseFilterEnum.Ignore
        };
        drum.AimAt(1);
        wheel.AddChild(drum);

        var playerCard = selectedHandIndex == 0;
        AddChoice(wheel, drum, 0,
            playerCard ? GameText.Pick("Сыграть", "Play") : GameText.Pick("Подсказать цвет", "Hint color"),
            playerCard ? CanPlaySelected() : CanHintSelected(), playerCard ? PlaySelected : HintSelectedColor);
        AddChoice(wheel, drum, 1,
            playerCard ? GameText.Pick("Сбросить", "Discard") : GameText.Pick("Подсказать ранг", "Hint rank"),
            playerCard ? CanDropSelected() : CanHintSelected(), playerCard ? DropSelected : HintSelectedRank);
        AddChoice(wheel, drum, 2, GameText.Pick("Назад", "Back"), true, CloseCardOverlay);
        return wheel;
    }

    private static void AddChoice(Control wheel, RevolverDrum drum, int index, string text, bool enabled, Action action)
    {
        var button = new Button
        {
            Position = new Vector2(index == 1 ? 186 : 166, 28 + index * 54),
            CustomMinimumSize = new Vector2(166, 42),
            Size = new Vector2(166, 42),
            Text = text,
            Disabled = !enabled,
            FocusMode = FocusModeEnum.None
        };
        button.MouseEntered += () => drum.AimAt(index);
        button.Pressed += action;
        wheel.AddChild(button);
    }

    private string? SelectedCardTexturePath()
    {
        if (selectedHandIndex is null || selectedCardIndex is null)
        {
            return null;
        }

        var hand = game.Hands[selectedHandIndex.Value];
        if (selectedCardIndex.Value < 0 || selectedCardIndex.Value >= hand.Count)
        {
            return null;
        }

        if (selectedHandIndex == 0)
        {
            return CardBackTexturePath;
        }

        var card = hand.Slots[selectedCardIndex.Value].Card;
        var path = CardTexturePath(card.Color, card.Rank);
        return ResourceLoader.Exists(path) ? path : null;
    }

    private bool IsHintedCard(Card card, int handIndex)
    {
        return hintedHandIndex == handIndex &&
            ((hintedColor is not null && card.Color == hintedColor) ||
             (hintedRank is not null && card.Rank == hintedRank));
    }

    private string HintMarker()
    {
        return hintedColor is not null ? WesternSymbol(hintedColor.Value) : hintedRank?.ToString() ?? "";
    }

    private Control CreatePlanLane(int lane)
    {
        var panel = new PlanLaneGuide
        {
            BurnedFromRank = burnedFromRank[lane] ?? 0,
            Progress = laneProgress[lane],
            CustomMinimumSize = PlanLaneSize,
            Size = PlanLaneSize,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };

        for (var rank = 1; rank <= laneProgress[lane]; rank++)
        {
            var card = CreatePlayedCardImage(LaneColors[lane], rank);
            if (card is not null)
            {
                card.Position = new Vector2(20, 24 + (rank - 1) * 22);
                card.RotationDegrees = PlayedCardRotation(LaneColors[lane], rank);
                card.PivotOffset = PlayedCardSize / 2f;
                panel.AddChild(card);
            }
        }

        return panel;
    }

    private static float PlayedCardRotation(CardColor color, int rank)
    {
        return (((int)color * 31 + rank * 17) % 21) - 10;
    }

    private Button? CreatePlayedCardImage(CardColor color, int rank)
    {
        var path = CardTexturePath(color, rank);
        if (!ResourceLoader.Exists(path))
        {
            return null;
        }

        var target = PlanCardFlightRect(new Card(color, rank));
        var button = new DelayedCardButton
        {
            RevealAt = CardRevealAt(target),
            CustomMinimumSize = PlayedCardSize,
            Size = PlayedCardSize,
            Icon = GD.Load<Texture2D>(path),
            ExpandIcon = true,
            FocusMode = FocusModeEnum.None
        };
        button.AddThemeColorOverride("icon_normal_color", Colors.White);
        button.AddThemeColorOverride("icon_hover_color", Colors.White);
        button.AddThemeColorOverride("icon_pressed_color", Colors.White);
        button.AddThemeStyleboxOverride("normal", EmptyStyle());
        button.AddThemeStyleboxOverride("hover", EmptyStyle());
        button.AddThemeStyleboxOverride("pressed", EmptyStyle());
        button.AddThemeStyleboxOverride("focus", EmptyStyle());
        button.Pressed += () =>
        {
            viewedCardTexturePath = path;
            RefreshTable();
        };
        return button;
    }

    private Button CreateActionButton(string text, bool enabled, Action pressed)
    {
        var button = new Button
        {
            Text = text,
            Disabled = !enabled,
            CustomMinimumSize = new Vector2(104, 36)
        };
        button.Pressed += pressed;
        return button;
    }

    private Control CreateDeckPile()
    {
        var pile = new Control
        {
            CustomMinimumSize = StockDeckSize + new Vector2(6, 16),
            Size = StockDeckSize + new Vector2(6, 16)
        };

        for (var i = 2; i >= 0; i--)
        {
            var image = CreateTokenImage(DeckTexturePath, new Vector2(i * 3, i * 8), StockDeckSize);
            image.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
            image.Modulate = i == 0 ? Colors.White : new Color(0.72f, 0.66f, 0.56f);
            pile.AddChild(image);
        }

        var count = new Label
        {
            Size = StockDeckSize,
            Text = game.DrawDeck.Count.ToString(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        count.AddThemeColorOverride("font_color", Colors.White);
        count.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.86f));
        count.AddThemeConstantOverride("outline_size", 4);
        count.AddThemeFontSizeOverride("font_size", 28);
        pile.AddChild(count);

        return pile;
    }

    private static Control CreateHintTokens(int available)
    {
        var tokens = new Control
        {
            CustomMinimumSize = new Vector2(126, 168),
            Size = new Vector2(126, 168)
        };

        for (var i = 0; i < MaxHintTokens; i++)
        {
            var token = CreateTokenImage(
                HintTokenTexturePath,
                new Vector2((i % 2) * 42, (i / 2) * 28),
                StockTokenSize);
            token.Modulate = i < available ? Colors.White : new Color(0.22f, 0.18f, 0.14f, 0.42f);
            tokens.AddChild(token);
        }

        return tokens;
    }

    private Control CreateMistakeTokens()
    {
        var tokens = new Control
        {
            CustomMinimumSize = MistakeTokensSize,
            Size = MistakeTokensSize,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter
        };

        var bottleStanding = mistakesLeft > 0;
        tokens.AddChild(CreateTokenImage(
            bottleStanding ? WhiskeyFullTexturePath : WhiskeyEmptyTexturePath,
            bottleStanding ? new Vector2(0, 116) : new Vector2(24, 226),
            MistakeBottleSize));

        var fullGlasses = Math.Max(0, mistakesLeft - 1);
        for (var i = 0; i < 2; i++)
        {
            tokens.AddChild(CreateTokenImage(
                i < fullGlasses ? GlassFullTexturePath : GlassEmptyTexturePath,
                new Vector2(54 + i * 112, 0),
                MistakeGlassSize));
        }

        return tokens;
    }

    private static TextureRect CreateTokenImage(string texturePath, Vector2 position, Vector2 size)
    {
        return new TextureRect
        {
            Position = position,
            CustomMinimumSize = size,
            Size = size,
            Texture = GD.Load<Texture2D>(texturePath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore
        };
    }

    private Control CreateShareCounter()
    {
        var paper = new Button
        {
            Position = new Vector2(1000, 408),
            CustomMinimumSize = ShareNoteSize,
            Size = ShareNoteSize,
            Text = GameText.English
                ? $"THE CUT\nME {playerShare}%\nHIM {partnerShare}%\n{(Score() >= 16 ? "TAKE IT\nAND RUN" : $"{Score()}/25")}"
                : $"Доли\nМне {playerShare}%\nЕму {partnerShare}%\n{(Score() >= 16 ? "На том\nи порешим" : $"{Score()}/25")}",
            Disabled = Score() < 16 || mistakesLeft <= 0,
            FocusMode = FocusModeEnum.None,
            ZIndex = 60,
            RotationDegrees = -4
        };
        paper.AddThemeStyleboxOverride("normal", PanelStyle(new Color("d9c39a"), new Color("8a6238")));
        paper.AddThemeStyleboxOverride("hover", PanelStyle(new Color("ead3a5"), new Color("8a6238")));
        paper.AddThemeStyleboxOverride("pressed", PanelStyle(new Color("c8aa72"), new Color("8a6238")));
        paper.AddThemeStyleboxOverride("disabled", PanelStyle(new Color("d9c39a"), new Color("8a6238")));
        paper.AddThemeColorOverride("font_color", new Color("3e291b"));
        paper.AddThemeColorOverride("font_disabled_color", new Color("3e291b"));
        paper.AddThemeFontSizeOverride("font_size", 14);
        paper.Pressed += ConfirmCashOut;
        return paper;
    }

    private static StyleBoxFlat PanelStyle(Color background, Color border)
    {
        return new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            ContentMarginBottom = 6,
            ContentMarginLeft = 6,
            ContentMarginRight = 6,
            ContentMarginTop = 6
        };
    }

    private static StyleBoxEmpty EmptyStyle()
    {
        return new StyleBoxEmpty();
    }

    private static StyleBoxFlat SpeechBubbleStyle(Color background)
    {
        return new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = new Color("7d5a35"),
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            ContentMarginBottom = 6,
            ContentMarginLeft = 8,
            ContentMarginRight = 8,
            ContentMarginTop = 6
        };
    }

    private static Control CreateColorHintBadge(CardColor color, Vector2 cardSize)
    {
        var textColor = color is CardColor.Yellow or CardColor.White ? new Color("2b2117") : Colors.White;
        return CreateHintBadge(WesternSymbol(color), CardTint(color), textColor, cardSize);
    }

    private static Control CreateRankHintBadge(int rank, Vector2 cardSize)
    {
        return CreateHintBadge(rank.ToString(), new Color(0.08f, 0.06f, 0.04f, 0.82f), Colors.White, cardSize);
    }

    private static Control CreateHintBadge(string text, Color background, Color textColor, Vector2 cardSize)
    {
        var badge = new Panel
        {
            Position = new Vector2(8, 8),
            Size = new Vector2(32, 32),
            MouseFilter = MouseFilterEnum.Ignore
        };
        badge.AddThemeStyleboxOverride("panel", HintBadgeStyle(background));

        var label = new Label
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        label.AddThemeColorOverride("font_color", textColor);
        label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.65f));
        label.AddThemeConstantOverride("outline_size", 2);
        label.AddThemeFontSizeOverride("font_size", 20);
        badge.AddChild(label);
        return badge;
    }

    private static StyleBoxFlat HintBadgeStyle(Color background)
    {
        return new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = new Color("f6e3a6"),
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            CornerRadiusBottomLeft = 16,
            CornerRadiusBottomRight = 16,
            CornerRadiusTopLeft = 16,
            CornerRadiusTopRight = 16,
            ContentMarginBottom = 0,
            ContentMarginLeft = 0,
            ContentMarginRight = 0,
            ContentMarginTop = 0
        };
    }

    private static int LaneFor(CardColor color)
    {
        return Array.IndexOf(LaneColors, color);
    }

    private static Color CardTint(CardColor color)
    {
        return color switch
        {
            CardColor.Red => new Color("c73e3a"),
            CardColor.Green => new Color("3f8f52"),
            CardColor.Blue => new Color("2f6fb8"),
            CardColor.Yellow => new Color("d2a532"),
            CardColor.White => new Color("e8e4d7"),
            _ => Colors.Gray
        };
    }

    private static string WesternSymbol(CardColor color)
    {
        return color switch
        {
            CardColor.Red => "R",
            CardColor.Green => "^",
            CardColor.Blue => "~",
            CardColor.Yellow => "$",
            CardColor.White => "*",
            _ => "?"
        };
    }

    private static string CardTexturePath(CardColor color, int rank)
    {
        var prefix = color switch
        {
            CardColor.Red => "R",
            CardColor.Green => "G",
            CardColor.Blue => "B",
            CardColor.Yellow => "Y",
            CardColor.White => "W",
            _ => "?"
        };
        var languageFolder = GameText.English ? "en/" : "";
        return $"res://assets/{languageFolder}{prefix}{rank}.png";
    }

    private static string CardText(Card card)
    {
        return $"{WesternSymbol(card.Color)}{card.Rank}";
    }

    private partial class SpeechBubbleTail : Control
    {
        public override void _Draw()
        {
            var fill = new Color("f2e3bf");
            var border = new Color("7d5a35");
            Vector2[] points = [new(0, 0), new(Size.X, 0), new(Size.X * 0.18f, Size.Y)];
            DrawColoredPolygon(points, fill);
            DrawPolyline([points[0], points[2], points[1]], border, 2f);
        }
    }

    private partial class BreathingFigure : TextureRect
    {
        public override void _Process(double delta)
        {
            var breath = (Mathf.Sin((float)(Time.GetTicksMsec() / 1000.0) * Mathf.Tau / PartnerBreathPeriod) + 1f) / 2f;
            Scale = new Vector2(1f + breath * 0.002f, 1f + breath * PartnerBreathPixels / Size.Y);
        }
    }

    private partial class DelayedCardButton : Button
    {
        public double RevealAt { get; init; }

        public override void _Ready()
        {
            if (RevealAt <= Time.GetTicksMsec() / 1000.0)
            {
                SetProcess(false);
                return;
            }

            Modulate = new Color(1, 1, 1, 0);
        }

        public override void _Process(double delta)
        {
            if (Time.GetTicksMsec() / 1000.0 < RevealAt)
            {
                return;
            }

            Modulate = Colors.White;
            SetProcess(false);
        }
    }

    private sealed record CardFlight(
        string StartTexturePath,
        string TargetTexturePath,
        Vector2 Start,
        Vector2 Target,
        Vector2 StartSize,
        Vector2 TargetSize,
        float StartRotation,
        float TargetRotation,
        bool Flip,
        double StartedAt)
    {
        public bool IsDone => Time.GetTicksMsec() / 1000.0 - StartedAt >= CardFlightDuration;
    }

    private partial class FlyingCard : TextureRect
    {
        private Texture2D? startTexture;
        private Texture2D? targetTexture;

        public CardFlight Flight { get; init; } = null!;

        public override void _Ready()
        {
            startTexture = GD.Load<Texture2D>(Flight.StartTexturePath);
            targetTexture = GD.Load<Texture2D>(Flight.TargetTexturePath);
            Texture = startTexture;
            ExpandMode = ExpandModeEnum.IgnoreSize;
            StretchMode = StretchModeEnum.KeepAspectCentered;
            MouseFilter = MouseFilterEnum.Ignore;
        }

        public override void _Process(double delta)
        {
            var elapsed = (float)(Time.GetTicksMsec() / 1000.0 - Flight.StartedAt);
            var t = Mathf.Clamp(elapsed / (float)CardFlightDuration, 0f, 1f);
            var eased = t * t * (3f - 2f * t);
            var size = Flight.StartSize.Lerp(Flight.TargetSize, eased);

            Position = Flight.Start.Lerp(Flight.Target, eased);
            Size = size;
            PivotOffset = size / 2f;
            RotationDegrees = Mathf.Lerp(Flight.StartRotation, Flight.TargetRotation, eased);
            Texture = Flight.Flip && t >= 0.5f ? targetTexture : startTexture;
            Scale = new Vector2(Flight.Flip ? Mathf.Max(0.08f, Mathf.Abs(Mathf.Cos(t * Mathf.Pi))) : 1f, 1f);
            Modulate = new Color(1, 1, 1, t > 0.86f ? 1f - (t - 0.86f) / 0.14f : 1f);

            if (t >= 1f)
            {
                QueueFree();
            }
        }
    }

    private partial class PlanLaneGuide : Control
    {
        public int BurnedFromRank { get; set; }
        public int Progress { get; set; }

        public override void _Draw()
        {
            var top = 10f;
            var bottom = Size.Y - 6f;
            var left = 6f;
            var right = Size.X - 6f;
            var highlight = new Color(0.9f, 0.78f, 0.45f, 0.46f);
            var shadow = new Color(0.12f, 0.07f, 0.03f, 0.42f);

            DrawRect(new Rect2(left, top, right - left, bottom - top), new Color(0.95f, 0.82f, 0.48f, 0.05f));
            DrawLine(new Vector2(left, top), new Vector2(left, bottom), shadow, 3f);
            DrawLine(new Vector2(left + 3f, top), new Vector2(left + 3f, bottom), highlight, 1.5f);
            DrawLine(new Vector2(right, top), new Vector2(right, bottom), shadow, 3f);
            DrawLine(new Vector2(right - 3f, top), new Vector2(right - 3f, bottom), highlight, 1.5f);

            for (var y = top + 30f; y < bottom; y += 42f)
            {
                DrawLine(new Vector2(left + 12f, y), new Vector2(right - 14f, y - 5f), new Color(0.95f, 0.82f, 0.48f, 0.18f), 1.2f);
            }

            if (BurnedFromRank <= 0)
            {
                return;
            }

            var crossTop = Mathf.Min(bottom - 86f, 30f + Progress * 22f + 105f);
            var crossBottom = bottom - 10f;
            DrawCut(new Vector2(left + 16f, crossTop), new Vector2(right - 16f, crossBottom), shadow, highlight);
            DrawCut(new Vector2(right - 14f, crossTop + 2f), new Vector2(left + 14f, crossBottom - 2f), shadow, highlight);
        }

        private void DrawCut(Vector2 from, Vector2 to, Color shadow, Color highlight)
        {
            DrawLine(from, to, shadow, 5f);
            DrawLine(from + new Vector2(2, -1), to + new Vector2(2, -1), highlight, 1.8f);
        }
    }

    private partial class RevolverDrum : TextureRect
    {
        private float targetRotation = Mathf.Pi / 2f;

        public void AimAt(int index)
        {
            targetRotation = Mathf.Pi / 2f + index switch
            {
                0 => -0.65f,
                1 => 0f,
                2 => 0.65f,
                _ => 0f
            };
        }

        public override void _Process(double delta)
        {
            Rotation = Mathf.LerpAngle(Rotation, targetRotation, Mathf.Clamp((float)delta * 12f, 0f, 1f));
        }
    }
}
