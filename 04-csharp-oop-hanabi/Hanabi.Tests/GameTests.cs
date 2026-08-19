using Hanabi.Core;

namespace Hanabi.Tests;

public class GameTests
{
    [Fact]
    public void Constructor_DealsFirstFiveCardsToPlayerOne()
    {
        var cards = OrderedCards();

        var game = new Game(cards);

        Assert.Equal(cards.Take(5), game.Hands[0].Slots.Select(slot => slot.Card));
    }

    [Fact]
    public void Constructor_DealsNextFiveCardsToPlayerTwo()
    {
        var cards = OrderedCards();

        var game = new Game(cards);

        Assert.Equal(cards.Skip(5).Take(5), game.Hands[1].Slots.Select(slot => slot.Card));
    }

    [Fact]
    public void Constructor_KeepsRemainingCardsInDrawDeckOrder()
    {
        var cards = OrderedCards();

        var game = new Game(cards);

        Assert.Equal(cards[10], game.DrawDeck.Draw());
        Assert.Equal(cards[11], game.DrawDeck.Draw());
        Assert.Equal(cards[12], game.DrawDeck.Draw());
    }

    [Fact]
    public void Constructor_SetsInitialCurrentPlayerToPlayerOne()
    {
        var game = new Game(OrderedCards());

        Assert.Equal(0, game.CurrentPlayerIndex);
    }

    [Fact]
    public void Constructor_SetsInitialCountersAndState()
    {
        var game = new Game(OrderedCards());

        Assert.Equal(0, game.MoveNumber);
        Assert.Equal(0, game.RiskyPlayCount);
        Assert.False(game.IsOver);
        Assert.Equal(0, game.Tableau.Count);
        Assert.Equal(0, game.DiscardPile.Count);
    }

    [Fact]
    public void Constructor_RejectsTooShortInput()
    {
        var cards = OrderedCards().Take(10);

        var exception = Assert.Throws<ArgumentException>(() => new Game(cards));
        Assert.Equal("cards", exception.ParamName);
    }

    private static List<Card> OrderedCards()
    {
        return
        [
            new Card(CardColor.Red, 1),
            new Card(CardColor.Red, 2),
            new Card(CardColor.Red, 3),
            new Card(CardColor.Red, 4),
            new Card(CardColor.Red, 5),
            new Card(CardColor.Blue, 1),
            new Card(CardColor.Blue, 2),
            new Card(CardColor.Blue, 3),
            new Card(CardColor.Blue, 4),
            new Card(CardColor.Blue, 5),
            new Card(CardColor.Green, 1),
            new Card(CardColor.Yellow, 1),
            new Card(CardColor.White, 1),
        ];
    }
}
