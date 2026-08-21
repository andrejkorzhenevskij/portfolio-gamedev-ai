using Godot;
using Hanabi.Core;

public partial class Main : Control
{
    private static readonly Vector2 CardSize = new(92, 128);

    public override void _Ready()
    {
        var game = new Hanabi.Core.Game(CreateStartingCards());

        var table = new Panel
        {
            AnchorRight = 1,
            AnchorBottom = 1,
        };
        table.AddThemeStyleboxOverride("panel", PanelStyle(new Color("24452f"), new Color("183020")));
        AddChild(table);

        var layout = new VBoxContainer
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetLeft = 28,
            OffsetTop = 22,
            OffsetRight = -28,
            OffsetBottom = -22,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        AddChild(layout);

        layout.AddChild(CreatePartnerHandArea(game));
        AddSpacer(layout, 20);

        var tableZones = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        tableZones.AddChild(CreateMistakesArea(game));
        tableZones.AddChild(CreatePlanArea(game));
        tableZones.AddChild(CreateDeckArea(game));
        layout.AddChild(tableZones);

        var bottomZones = new VBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        bottomZones.AddChild(CreatePlayerHandArea(game));
        bottomZones.AddChild(CreateActionsArea());
        layout.AddChild(bottomZones);
    }

    private static Card[] CreateStartingCards()
    {
        return
        [
            new(CardColor.Red, 1),
            new(CardColor.Green, 1),
            new(CardColor.Blue, 1),
            new(CardColor.Yellow, 1),
            new(CardColor.White, 1),
            new(CardColor.Red, 2),
            new(CardColor.Green, 2),
            new(CardColor.Blue, 2),
            new(CardColor.Yellow, 2),
            new(CardColor.White, 2),
            new(CardColor.Red, 3)
        ];
    }

    private static Control CreatePartnerHandArea(Hanabi.Core.Game game)
    {
        var area = CreateZone("PartnerHand");
        AddTitle(area, "Partner");
        AddHand(area, game.Hands[1], hideCards: true);
        return area;
    }

    private static Control CreatePlayerHandArea(Hanabi.Core.Game game)
    {
        var area = CreateZone("PlayerHand");
        AddTitle(area, "You");
        AddHand(area, game.Hands[0], hideCards: false);
        return area;
    }

    private static Control CreateMistakesArea(Hanabi.Core.Game game)
    {
        var area = CreateZone("Mistakes");
        area.AddChild(CreatePile("Failed attempts", game.DiscardPile.Count.ToString(), new Color("5a2630")));
        return area;
    }

    private static Control CreatePlanArea(Hanabi.Core.Game game)
    {
        var area = CreateZone("Plan");
        area.AddChild(CreatePile("Plan", $"{game.Tableau.Count}/25", new Color("2f6241")));
        return area;
    }

    private static Control CreateDeckArea(Hanabi.Core.Game game)
    {
        var area = CreateZone("Deck");
        area.AddChild(CreatePile("Stagecoach", game.DrawDeck.Count.ToString(), new Color("4a3428")));
        return area;
    }

    private static Control CreateActionsArea()
    {
        var area = CreateZone("Actions");
        area.AddChild(new Button
        {
            Text = "Action",
            CustomMinimumSize = new Vector2(130, 48)
        });
        return area;
    }

    private static VBoxContainer CreateZone(string name)
    {
        return new VBoxContainer
        {
            Name = name,
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
    }

    private static void AddTitle(VBoxContainer layout, string text)
    {
        var label = new Label
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        label.AddThemeFontSizeOverride("font_size", 22);
        label.AddThemeColorOverride("font_color", Colors.White);
        layout.AddChild(label);
    }

    private static void AddHand(VBoxContainer layout, Hand hand, bool hideCards)
    {
        var row = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };

        foreach (var slot in hand.Slots)
        {
            row.AddChild(CreateCard(slot.Card, hideCards));
        }

        layout.AddChild(row);
    }

    private static Control CreateCard(Card card, bool hidden)
    {
        var panel = new Panel { CustomMinimumSize = CardSize };
        panel.AddThemeStyleboxOverride(
            "panel",
            hidden
                ? PanelStyle(new Color("26324f"), new Color("d6c07a"))
                : PanelStyle(CardTint(card.Color), Colors.White));

        var rank = new Label
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            Text = hidden ? "?" : WesternSymbol(card.Color),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        rank.AddThemeFontSizeOverride("font_size", 42);
        rank.AddThemeColorOverride("font_color", hidden ? new Color("d6c07a") : Colors.White);
        panel.AddChild(rank);

        return panel;
    }

    private static Control CreatePile(string title, string value, Color color)
    {
        var pile = new Panel { CustomMinimumSize = new Vector2(120, 128) };
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
        valueLabel.AddThemeFontSizeOverride("font_size", 30);
        valueLabel.AddThemeColorOverride("font_color", Colors.White);
        labels.AddChild(valueLabel);

        return pile;
    }

    private static void AddSpacer(VBoxContainer layout, float height)
    {
        layout.AddChild(new Control { CustomMinimumSize = new Vector2(1, height) });
    }

    private static StyleBoxFlat PanelStyle(Color background, Color border)
    {
        return new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            BorderWidthBottom = 3,
            BorderWidthLeft = 3,
            BorderWidthRight = 3,
            BorderWidthTop = 3,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            ContentMarginBottom = 12,
            ContentMarginLeft = 12,
            ContentMarginRight = 12,
            ContentMarginTop = 12
        };
    }

    private static Color CardTint(CardColor color)
    {
        return color switch
        {
            Hanabi.Core.CardColor.Red => new Color("c73e3a"),
            Hanabi.Core.CardColor.Green => new Color("3f8f52"),
            Hanabi.Core.CardColor.Blue => new Color("2f6fb8"),
            Hanabi.Core.CardColor.Yellow => new Color("d2a532"),
            Hanabi.Core.CardColor.White => new Color("e8e4d7"),
            _ => Colors.Gray
        };
    }

    private static string WesternSymbol(CardColor color)
    {
        return color switch
        {
            Hanabi.Core.CardColor.Red => "R",
            Hanabi.Core.CardColor.Blue => "~",
            Hanabi.Core.CardColor.Green => "^",
            Hanabi.Core.CardColor.Yellow => "$",
            Hanabi.Core.CardColor.White => "*",
            _ => "?"
        };
    }
}
