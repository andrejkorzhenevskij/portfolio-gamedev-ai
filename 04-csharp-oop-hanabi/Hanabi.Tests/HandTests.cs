using Hanabi.Core;

namespace Hanabi.Tests;

public class HandTests
{
    [Fact]
    public void Count_StartsAtZero()
    {
        var hand = new Hand();

        Assert.Equal(0, hand.Count);
    }

    [Fact]
    public void Add_StoresCardsInOrder()
    {
        var first = new Card(CardColor.Red, 1);
        var second = new Card(CardColor.Blue, 5);
        var hand = new Hand();

        hand.Add(first);
        hand.Add(second);

        Assert.Equal(2, hand.Count);
        Assert.Equal([first, second], hand.Cards);
    }

    [Fact]
    public void RemoveAt_ReturnsAndRemovesCard()
    {
        var first = new Card(CardColor.Red, 1);
        var second = new Card(CardColor.Blue, 5);
        var hand = new Hand();
        hand.Add(first);
        hand.Add(second);

        var removed = hand.RemoveAt(0);

        Assert.Equal(first, removed);
        Assert.Equal(1, hand.Count);
        Assert.Equal([second], hand.Cards);
    }
}
