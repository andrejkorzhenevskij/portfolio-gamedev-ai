using Hanabi.Core;

namespace Hanabi.Tests;

public class DeckTests
{
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
