using Hanabi.Core;

namespace Hanabi.Tests;

public class DiscardPileTests
{
    [Fact]
    public void Count_StartsAtZero()
    {
        var pile = new DiscardPile();

        Assert.Equal(0, pile.Count);
    }

    [Fact]
    public void Add_StoresDiscardedCardsInOrder()
    {
        var first = new Card(CardColor.Red, 1);
        var second = new Card(CardColor.Blue, 5);
        var pile = new DiscardPile();

        pile.Add(first);
        pile.Add(second);

        Assert.Equal(2, pile.Count);
        Assert.Equal([first, second], pile.Cards);
    }
}
