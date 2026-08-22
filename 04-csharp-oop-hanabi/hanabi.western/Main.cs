using Godot;
using Hanabi.Core;

public partial class Main : Control
{
    private const int MaxMistakes = 3;
    private static readonly Vector2 ScreenSize = new(1152, 648);
    private static readonly Vector2 LayoutPadding = new(18, 10);
    private static readonly Vector2 CardSize = new(72, 96);
    private static readonly Vector2 PlanLaneSize = new(116, 214);
    private static readonly string[] PlanLanes = ["Маршрут", "Оружие", "Охрана", "Цель", "Отход"];
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
    private int mistakesLeft = MaxMistakes;
    private int? selectedHandIndex;
    private int? selectedCardIndex;

    public override void _Ready()
    {
        GetWindow().Size = (Vector2I)ScreenSize;
        game = new Hanabi.Core.Game(CreateStartingCards());
        RefreshTable();
    }

    private void RefreshTable()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }

        var table = new Panel { CustomMinimumSize = ScreenSize, Size = ScreenSize };
        table.AddThemeStyleboxOverride("panel", PanelStyle(new Color("6f4527"), new Color("3f2515")));
        AddChild(table);

        var layoutSize = ScreenSize - LayoutPadding * 2;
        var layout = new VBoxContainer
        {
            Position = LayoutPadding,
            CustomMinimumSize = layoutSize,
            Size = layoutSize
        };
        layout.AddThemeConstantOverride("separation", 8);
        AddChild(layout);

        layout.AddChild(CreatePartnerHandArea());
        layout.AddChild(CreateCenterRow());
        layout.AddChild(CreatePlayerHandArea());
        layout.AddChild(CreateActionsArea());
    }

    private static Card[] CreateStartingCards()
    {
        return
        [
            .. from rank in Enumerable.Range(1, 5)
               from color in LaneColors
               from _ in Enumerable.Range(0, rank == 1 ? 3 : rank == 5 ? 1 : 2)
               select new Card(color, rank)
        ];
    }

    private Control CreatePartnerHandArea()
    {
        var area = CreateZone("PartnerHand", new Vector2(1, 118));
        AddTitle(area, "Партнёр");
        AddHand(area, game.Hands[1], handIndex: 1, hideCards: false);
        return area;
    }

    private Control CreateCenterRow()
    {
        var row = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            CustomMinimumSize = new Vector2(1, 248),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        row.AddThemeConstantOverride("separation", 12);
        row.AddChild(CreateMistakesArea());
        row.AddChild(CreatePlanArea());
        row.AddChild(CreateDeckArea());
        return row;
    }

    private Control CreatePlayerHandArea()
    {
        var area = CreateZone("PlayerHand", new Vector2(1, 104));
        AddHand(area, game.Hands[0], handIndex: 0, hideCards: true);
        return area;
    }

    private Control CreateMistakesArea()
    {
        var area = CreateZone("Mistakes", new Vector2(128, 1));
        AddTitle(area, "Ошибки");
        area.AddChild(CreatePile("Осталось", mistakesLeft.ToString(), new Color("5a2630")));
        return area;
    }

    private Control CreatePlanArea()
    {
        var area = CreateZone("Plan", new Vector2(650, 1));
        AddTitle(area, "План");

        var lanes = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        lanes.AddThemeConstantOverride("separation", 8);

        for (var lane = 0; lane < PlanLanes.Length; lane++)
        {
            lanes.AddChild(CreatePlanLane(lane));
        }

        area.AddChild(lanes);
        return area;
    }

    private Control CreateDeckArea()
    {
        var area = CreateZone("Deck", new Vector2(142, 1));
        AddTitle(area, "Запасы");
        area.AddChild(CreatePile("Колода", game.DrawDeck.Count.ToString(), new Color("4a3428")));
        area.AddChild(CreateHintTokens());
        area.AddChild(CreateShareCounter());
        return area;
    }

    private Control CreateActionsArea()
    {
        var area = CreateZone("Actions", new Vector2(1, 46));
        var row = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        row.AddThemeConstantOverride("separation", 8);

        row.AddChild(CreateActionButton("Сыграть", CanPlaySelected(), PlaySelected));
        row.AddChild(CreateActionButton("Сбросить", CanDropSelected(), DropSelected));
        row.AddChild(CreateActionButton("Цвет", CanHintSelected(), HintSelectedColor));
        row.AddChild(CreateActionButton("Ранг", CanHintSelected(), HintSelectedRank));
        area.AddChild(row);
        return area;
    }

    private bool CanPlaySelected()
    {
        return mistakesLeft > 0 &&
            selectedHandIndex == 0 &&
            selectedCardIndex is not null;
    }

    private bool CanDropSelected()
    {
        return selectedHandIndex == 0 && selectedCardIndex is not null;
    }

    private bool CanHintSelected()
    {
        return selectedHandIndex == 1 && selectedCardIndex is not null;
    }

    private void PlaySelected()
    {
        if (!CanPlaySelected())
        {
            return;
        }

        var hand = game.Hands[0];
        var card = hand.Slots[selectedCardIndex!.Value].Card;
        hand.RemoveAt(selectedCardIndex.Value);

        var lane = LaneFor(card.Color);
        if (burnedFromRank[lane] is null && card.Rank == laneProgress[lane] + 1)
        {
            laneProgress[lane] = card.Rank;
        }
        else
        {
            mistakesLeft--;
            burnedFromRank[lane] ??= laneProgress[lane] + 1;
            RemoveColorFromHands(card.Color);
        }

        RefillHand(hand);
        ClearSelection();
        RefreshTable();
    }

    private void DropSelected()
    {
        if (!CanDropSelected())
        {
            return;
        }

        var hand = game.Hands[0];
        hand.RemoveAt(selectedCardIndex!.Value);
        RefillHand(hand);
        ClearSelection();
        RefreshTable();
    }

    private void HintSelectedColor()
    {
        ClearSelection();
        RefreshTable();
    }

    private void HintSelectedRank()
    {
        ClearSelection();
        RefreshTable();
    }

    private void RefillHand(Hand hand)
    {
        while (hand.Count < 5 && TryDrawPlayableCard(out var card))
        {
            hand.Add(card);
        }
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
        selectedHandIndex = handIndex;
        selectedCardIndex = cardIndex;
        RefreshTable();
    }

    private void ClearSelection()
    {
        selectedHandIndex = null;
        selectedCardIndex = null;
    }

    private static VBoxContainer CreateZone(string name, Vector2 minimumSize)
    {
        return new VBoxContainer
        {
            Name = name,
            Alignment = BoxContainer.AlignmentMode.Center,
            CustomMinimumSize = minimumSize,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
    }

    private static void AddTitle(VBoxContainer layout, string text)
    {
        var label = new Label
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        label.AddThemeFontSizeOverride("font_size", 18);
        label.AddThemeColorOverride("font_color", Colors.White);
        layout.AddChild(label);
    }

    private void AddHand(VBoxContainer layout, Hand hand, int handIndex, bool hideCards)
    {
        var row = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        row.AddThemeConstantOverride("separation", 8);

        for (var index = 0; index < hand.Slots.Count; index++)
        {
            row.AddChild(CreateCard(hand.Slots[index].Card, handIndex, index, hideCards));
        }

        layout.AddChild(row);
    }

    private Button CreateCard(Card card, int handIndex, int cardIndex, bool hidden)
    {
        var selected = selectedHandIndex == handIndex && selectedCardIndex == cardIndex;
        var blocked = burnedFromRank[LaneFor(card.Color)] is not null;
        var button = new Button
        {
            CustomMinimumSize = CardSize,
            Text = hidden ? "◆" : $"{WesternSymbol(card.Color)}\n{card.Rank}",
            Disabled = blocked
        };
        button.AddThemeFontSizeOverride("font_size", 26);
        button.AddThemeStyleboxOverride(
            "normal",
            hidden
                ? PanelStyle(blocked ? new Color("2b2528") : new Color("26324f"), selected ? new Color("f0d35f") : new Color("d6c07a"))
                : PanelStyle(blocked ? new Color("382a2a") : CardTint(card.Color), selected ? new Color("f0d35f") : Colors.White));
        button.AddThemeStyleboxOverride("pressed", PanelStyle(hidden ? new Color("26324f") : CardTint(card.Color), new Color("f0d35f")));
        button.Pressed += () => SelectCard(handIndex, cardIndex);
        return button;
    }

    private Control CreatePlanLane(int lane)
    {
        var panel = new Panel
        {
            CustomMinimumSize = PlanLaneSize,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        panel.AddThemeStyleboxOverride("panel", PanelStyle(new Color("2f3f38"), new Color("d6c07a")));

        var column = new VBoxContainer
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        column.AddThemeConstantOverride("separation", 4);
        panel.AddChild(column);

        var label = new Label { Text = PlanLanes[lane], HorizontalAlignment = HorizontalAlignment.Center };
        label.AddThemeColorOverride("font_color", Colors.White);
        label.AddThemeFontSizeOverride("font_size", 15);
        column.AddChild(label);

        for (var rank = 1; rank <= 5; rank++)
        {
            column.AddChild(CreateLaneCell(lane, rank));
        }

        return panel;
    }

    private Control CreateLaneCell(int lane, int rank)
    {
        var burned = burnedFromRank[lane] is not null && rank >= burnedFromRank[lane];
        var completed = rank <= laneProgress[lane];
        var cell = new Panel { CustomMinimumSize = new Vector2(1, 30) };
        cell.AddThemeStyleboxOverride(
            "panel",
            PanelStyle(
                burned ? new Color("3a2020") : completed ? CardTint(LaneColors[lane]) : new Color("1f2926"),
                burned ? new Color("d46a4a") : new Color("d6c07a")));

        var label = new Label
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            Text = burned ? "X" : completed ? $"{WesternSymbol(LaneColors[lane])}{rank}" : rank.ToString(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        label.AddThemeFontSizeOverride("font_size", 18);
        label.AddThemeColorOverride("font_color", burned ? new Color("ffb08f") : Colors.White);
        cell.AddChild(label);
        return cell;
    }

    private Button CreateActionButton(string text, bool enabled, Action pressed)
    {
        var button = new Button
        {
            Text = text,
            Disabled = !enabled,
            CustomMinimumSize = new Vector2(112, 38)
        };
        button.Pressed += pressed;
        return button;
    }

    private static Control CreatePile(string title, string value, Color color)
    {
        var pile = new Panel { CustomMinimumSize = new Vector2(108, 76) };
        pile.AddThemeStyleboxOverride("panel", PanelStyle(color, Colors.White));

        var labels = new VBoxContainer
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        pile.AddChild(labels);

        var titleLabel = new Label { Text = title, HorizontalAlignment = HorizontalAlignment.Center };
        titleLabel.AddThemeColorOverride("font_color", Colors.White);
        labels.AddChild(titleLabel);

        var valueLabel = new Label { Text = value, HorizontalAlignment = HorizontalAlignment.Center };
        valueLabel.AddThemeFontSizeOverride("font_size", 24);
        valueLabel.AddThemeColorOverride("font_color", Colors.White);
        labels.AddChild(valueLabel);

        return pile;
    }

    private static Control CreateHintTokens()
    {
        var grid = new GridContainer { Columns = 4 };
        grid.AddThemeConstantOverride("h_separation", 6);
        grid.AddThemeConstantOverride("v_separation", 6);

        for (var i = 0; i < 8; i++)
        {
            var token = new Panel { CustomMinimumSize = new Vector2(24, 24) };
            token.AddThemeStyleboxOverride("panel", PanelStyle(new Color("d2a532"), new Color("fff0a8")));
            grid.AddChild(token);
        }

        return grid;
    }

    private static Control CreateShareCounter()
    {
        var label = new Label
        {
            Text = "Доля\n50/50",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        label.AddThemeColorOverride("font_color", Colors.White);
        label.AddThemeFontSizeOverride("font_size", 16);
        return label;
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
}
