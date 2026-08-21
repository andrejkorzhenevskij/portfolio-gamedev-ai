using Godot;
using Hanabi.Core;

public partial class Main : Control
{
    public override void _Ready()
    {
        var game = new Hanabi.Core.Game(CreateStartingCards());

        var rows = new VBoxContainer
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetLeft = 24,
            OffsetTop = 24,
            OffsetRight = -24,
            OffsetBottom = -24
        };

        AddChild(rows);
        AddRow(rows, "Hanabi initial state");
        AddRow(rows, $"Current player: {game.CurrentPlayerIndex + 1}");
        AddRow(rows, $"Move: {game.MoveNumber}");
        AddRow(rows, $"Deck: {game.DrawDeck.Count} card(s)");
        AddRow(rows, $"Tableau: {game.Tableau.Count} played");
        AddRow(rows, $"Discard pile: {game.DiscardPile.Count} card(s)");

        for (var player = 0; player < game.Hands.Count; player++)
        {
            var cards = game.Hands[player].Slots.Select(slot => FormatCard(slot.Card));
            AddRow(rows, $"Player {player + 1}: {string.Join(", ", cards)}");
        }
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

    private static void AddRow(VBoxContainer rows, string text)
    {
        rows.AddChild(new Label { Text = text });
    }

    private static string FormatCard(Card card)
    {
        return $"{card.Color} {card.Rank}";
    }
}
