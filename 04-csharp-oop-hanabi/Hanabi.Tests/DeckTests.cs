using Hanabi.Core;

namespace Hanabi.Tests;

public class DeckTests
{
    [Fact]
    public void CreateStandard_HasFiftyCards()
    {
        var deck = Deck.CreateStandard();

        Assert.Equal(50, deck.Count);
    }

    [Fact]
    public void CreateStandard_HasHanabiRankDistributionForEachColor()
    {
        var deck = Deck.CreateStandard();
        var cards = new List<Card>();

        while (!deck.IsEmpty)
        {
            cards.Add(deck.Draw());
        }

        foreach (var color in Enum.GetValues<CardColor>())
        {
            Assert.Equal(3, cards.Count(card => card == new Card(color, 1)));
            Assert.Equal(2, cards.Count(card => card == new Card(color, 2)));
            Assert.Equal(2, cards.Count(card => card == new Card(color, 3)));
            Assert.Equal(2, cards.Count(card => card == new Card(color, 4)));
            Assert.Equal(1, cards.Count(card => card == new Card(color, 5)));
        }
    }

    [Fact]
    public void Draw_ReturnsCardsInProvidedOrder()
    {
        var first = new Card(CardColor.Red, 1);
        var second = new Card(CardColor.Blue, 2);
        var deck = new Deck([first, second]);

        Assert.Equal(first, deck.Draw());
        Assert.Equal(second, deck.Draw());
    }

    [Fact]
    public void CountAndIsEmpty_ReflectRemainingCards()
    {
        var deck = new Deck([new Card(CardColor.Green, 3)]);

        Assert.Equal(1, deck.Count);
        Assert.False(deck.IsEmpty);

        deck.Draw();

        Assert.Equal(0, deck.Count);
        Assert.True(deck.IsEmpty);
    }

    [Fact]
    public void Draw_ThrowsWhenDeckIsEmpty()
    {
        var deck = new Deck([]);

        var exception = Assert.Throws<InvalidOperationException>(() => deck.Draw());
        Assert.Equal("Cannot draw from an empty deck.", exception.Message);
    }
}
