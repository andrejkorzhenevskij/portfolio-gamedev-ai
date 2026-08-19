using Hanabi.Core;

namespace Hanabi.Tests;

public class TableauTests
{
    [Fact]
    public void CanPlay_RankOneOnEmptyTableau()
    {
        var tableau = new Tableau();

        Assert.True(tableau.CanPlay(new Card(CardColor.Red, 1)));
    }

    [Fact]
    public void CanPlay_RankTwoOnlyAfterRankOne()
    {
        var tableau = new Tableau();
        var rankTwo = new Card(CardColor.Red, 2);

        Assert.False(tableau.CanPlay(rankTwo));

        tableau.Play(new Card(CardColor.Red, 1));

        Assert.True(tableau.CanPlay(rankTwo));
    }

    [Fact]
    public void CanPlay_DuplicateRankIsFalse()
    {
        var tableau = new Tableau();
        var card = new Card(CardColor.Red, 1);

        tableau.Play(card);

        Assert.False(tableau.CanPlay(card));
    }

    [Fact]
    public void CanPlay_SkippedRankIsFalse()
    {
        var tableau = new Tableau();
        tableau.Play(new Card(CardColor.Red, 1));

        Assert.False(tableau.CanPlay(new Card(CardColor.Red, 3)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void CanPlay_InvalidRankIsFalse(int rank)
    {
        var tableau = new Tableau();

        Assert.False(tableau.CanPlay(new Card(CardColor.Red, rank)));
    }

    [Fact]
    public void ColorsProgressIndependently()
    {
        var tableau = new Tableau();
        tableau.Play(new Card(CardColor.Red, 1));

        Assert.True(tableau.CanPlay(new Card(CardColor.Red, 2)));
        Assert.True(tableau.CanPlay(new Card(CardColor.Blue, 1)));
        Assert.False(tableau.CanPlay(new Card(CardColor.Blue, 2)));
    }

    [Fact]
    public void Count_TracksTotalPlayedCards()
    {
        var tableau = new Tableau();

        Assert.Equal(0, tableau.Count);

        tableau.Play(new Card(CardColor.Red, 1));
        tableau.Play(new Card(CardColor.Red, 2));
        tableau.Play(new Card(CardColor.Blue, 1));

        Assert.Equal(3, tableau.Count);
    }
}
