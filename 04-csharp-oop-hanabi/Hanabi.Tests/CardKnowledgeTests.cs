using Hanabi.Core;

namespace Hanabi.Tests;

public class CardKnowledgeTests
{
    [Fact]
    public void NewKnowledge_AllColorsAndRanksArePossible()
    {
        var knowledge = new CardKnowledge();

        Assert.Equal(Enum.GetValues<CardColor>(), knowledge.PossibleColors);
        Assert.Equal([1, 2, 3, 4, 5], knowledge.PossibleRanks);
    }

    [Fact]
    public void ApplyColorHint_WhenMatching_LeavesOnlyThatColor()
    {
        var knowledge = new CardKnowledge();

        knowledge.ApplyColorHint(CardColor.Blue, isMatching: true);

        Assert.Equal([CardColor.Blue], knowledge.PossibleColors);
    }

    [Fact]
    public void ApplyColorHint_WhenNotMatching_RemovesThatColor()
    {
        var knowledge = new CardKnowledge();

        knowledge.ApplyColorHint(CardColor.Blue, isMatching: false);

        Assert.DoesNotContain(CardColor.Blue, knowledge.PossibleColors);
        Assert.Equal(4, knowledge.PossibleColors.Count);
    }

    [Fact]
    public void ApplyRankHint_WhenMatching_LeavesOnlyThatRank()
    {
        var knowledge = new CardKnowledge();

        knowledge.ApplyRankHint(3, isMatching: true);

        Assert.Equal([3], knowledge.PossibleRanks);
    }

    [Fact]
    public void ApplyRankHint_WhenNotMatching_RemovesThatRank()
    {
        var knowledge = new CardKnowledge();

        knowledge.ApplyRankHint(3, isMatching: false);

        Assert.DoesNotContain(3, knowledge.PossibleRanks);
        Assert.Equal(4, knowledge.PossibleRanks.Count);
    }

    [Fact]
    public void Hints_CanBeCombined()
    {
        var knowledge = new CardKnowledge();

        knowledge.ApplyColorHint(CardColor.Red, isMatching: false);
        knowledge.ApplyColorHint(CardColor.Blue, isMatching: true);
        knowledge.ApplyRankHint(1, isMatching: false);
        knowledge.ApplyRankHint(5, isMatching: false);

        Assert.Equal([CardColor.Blue], knowledge.PossibleColors);
        Assert.Equal([2, 3, 4], knowledge.PossibleRanks);
    }
}
