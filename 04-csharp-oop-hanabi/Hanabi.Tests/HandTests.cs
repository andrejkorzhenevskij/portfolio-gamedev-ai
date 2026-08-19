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
    public void Add_CreatesSlotWithFreshKnowledge()
    {
        var card = new Card(CardColor.Red, 1);
        var hand = new Hand();

        hand.Add(card);

        Assert.Equal(1, hand.Count);
        Assert.Equal(card, hand.Slots[0].Card);
        Assert.Equal(Enum.GetValues<CardColor>(), hand.Slots[0].Knowledge.PossibleColors);
        Assert.Equal([1, 2, 3, 4, 5], hand.Slots[0].Knowledge.PossibleRanks);
    }

    [Fact]
    public void Add_CreatesIndependentKnowledgeForEachCard()
    {
        var hand = new Hand();
        hand.Add(new Card(CardColor.Red, 1));
        hand.Add(new Card(CardColor.Blue, 5));

        hand.Slots[0].Knowledge.ApplyColorHint(CardColor.Red, isMatching: true);

        Assert.NotSame(hand.Slots[0].Knowledge, hand.Slots[1].Knowledge);
        Assert.Equal([CardColor.Red], hand.Slots[0].Knowledge.PossibleColors);
        Assert.Equal(Enum.GetValues<CardColor>(), hand.Slots[1].Knowledge.PossibleColors);
    }

    [Fact]
    public void RemoveAt_ReturnsCardAndRemovesAssociatedKnowledge()
    {
        var first = new Card(CardColor.Red, 1);
        var second = new Card(CardColor.Blue, 5);
        var hand = new Hand();
        hand.Add(first);
        hand.Add(second);
        var removedKnowledge = hand.Slots[0].Knowledge;
        var remainingKnowledge = hand.Slots[1].Knowledge;

        var removed = hand.RemoveAt(0);

        Assert.Equal(first, removed);
        Assert.Equal(1, hand.Count);
        Assert.DoesNotContain(removedKnowledge, hand.Slots.Select(slot => slot.Knowledge));
        Assert.Same(remainingKnowledge, hand.Slots[0].Knowledge);
        Assert.Equal(second, hand.Slots[0].Card);
    }

    [Fact]
    public void RemoveAt_PreservesRemainingSlotOrder()
    {
        var first = new Card(CardColor.Red, 1);
        var second = new Card(CardColor.Blue, 5);
        var third = new Card(CardColor.Green, 3);
        var hand = new Hand();
        hand.Add(first);
        hand.Add(second);
        hand.Add(third);

        hand.RemoveAt(1);

        Assert.Equal([first, third], hand.Slots.Select(slot => slot.Card));
    }
}
