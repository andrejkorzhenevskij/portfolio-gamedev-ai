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

    [Fact]
    public void Drop_SendsSelectedCardToDiscardPile()
    {
        var cards = OrderedCards();
        var game = new Game(cards);

        game.Drop(2);

        Assert.Equal([cards[2]], game.DiscardPile.Cards);
    }

    [Fact]
    public void Drop_PreservesRemainingHandOrder()
    {
        var cards = OrderedCards();
        var game = new Game(cards);

        game.Drop(1);

        Assert.Equal([cards[0], cards[2], cards[3], cards[4], cards[10]], game.Hands[0].Slots.Select(slot => slot.Card));
    }

    [Fact]
    public void Drop_DrawsNextDeckCardToEndOfHand()
    {
        var cards = OrderedCards();
        var game = new Game(cards);

        game.Drop(0);

        Assert.Equal(cards[10], game.Hands[0].Slots[4].Card);
    }

    [Fact]
    public void Drop_DrawnCardGetsFreshKnowledge()
    {
        var cards = OrderedCards();
        var game = new Game(cards);
        game.Hands[0].ApplyColorHint(CardColor.Red);

        game.Drop(0);

        var drawnKnowledge = game.Hands[0].Slots[4].Knowledge;
        Assert.Equal(Enum.GetValues<CardColor>(), drawnKnowledge.PossibleColors);
        Assert.Equal([1, 2, 3, 4, 5], drawnKnowledge.PossibleRanks);
    }

    [Fact]
    public void Drop_DecreasesDrawDeckCount()
    {
        var game = new Game(OrderedCards());

        game.Drop(0);

        Assert.Equal(2, game.DrawDeck.Count);
    }

    [Fact]
    public void Drop_IncrementsMoveNumber()
    {
        var game = new Game(OrderedCards());

        game.Drop(0);

        Assert.Equal(1, game.MoveNumber);
    }

    [Fact]
    public void Drop_SwitchesCurrentPlayerAfterNormalDrop()
    {
        var game = new Game(OrderedCards());

        game.Drop(0);

        Assert.Equal(1, game.CurrentPlayerIndex);
    }

    [Fact]
    public void Drop_PlayerTwoCanDropOnTheirTurn()
    {
        var cards = OrderedCards();
        var game = new Game(cards);
        game.Drop(0);

        game.Drop(1);

        Assert.Equal([cards[0], cards[6]], game.DiscardPile.Cards);
    }

    [Fact]
    public void Drop_DrawingFinalDeckCardEndsGameOnThatMove()
    {
        var game = new Game(OrderedCards().Take(11));

        game.Drop(0);

        Assert.True(game.IsOver);
        Assert.Equal(1, game.MoveNumber);
        Assert.Equal(0, game.CurrentPlayerIndex);
    }

    [Fact]
    public void Play_WhenSuccessful_AddsSelectedCardToTableau()
    {
        var game = new Game(OrderedCards());

        game.Play(0);

        Assert.Equal(1, game.Tableau.Count);
        Assert.True(game.Tableau.CanPlay(new Card(CardColor.Red, 2)));
    }

    [Fact]
    public void Play_WhenSuccessful_PreservesRemainingHandOrderAndDrawsNextCard()
    {
        var cards = OrderedCards();
        var game = new Game(cards);

        game.Play(0);

        Assert.Equal([cards[1], cards[2], cards[3], cards[4], cards[10]], game.Hands[0].Slots.Select(slot => slot.Card));
    }

    [Fact]
    public void Play_WhenSuccessful_DrawnCardGetsFreshKnowledge()
    {
        var cards = OrderedCards();
        var game = new Game(cards);
        game.Hands[0].ApplyColorHint(CardColor.Red);

        game.Play(0);

        var drawnKnowledge = game.Hands[0].Slots[4].Knowledge;
        Assert.Equal(Enum.GetValues<CardColor>(), drawnKnowledge.PossibleColors);
        Assert.Equal([1, 2, 3, 4, 5], drawnKnowledge.PossibleRanks);
    }

    [Fact]
    public void Play_WhenSuccessful_IncrementsMoveNumberAndSwitchesPlayer()
    {
        var game = new Game(OrderedCards());

        game.Play(0);

        Assert.Equal(1, game.MoveNumber);
        Assert.Equal(1, game.CurrentPlayerIndex);
    }

    [Fact]
    public void Play_PlayerTwoCanSuccessfullyPlayOnTheirTurn()
    {
        var game = new Game(OrderedCards());
        game.Play(0);

        game.Play(0);

        Assert.Equal(2, game.Tableau.Count);
        Assert.Equal(2, game.MoveNumber);
        Assert.Equal(0, game.CurrentPlayerIndex);
    }

    [Fact]
    public void Play_WhenInvalid_AddsAttemptedCardToDiscardPile()
    {
        var cards = OrderedCards();
        var game = new Game(cards);

        game.Play(1);

        Assert.Equal([cards[1]], game.DiscardPile.Cards);
    }

    [Fact]
    public void Play_WhenInvalid_EndsGameImmediately()
    {
        var game = new Game(OrderedCards());

        game.Play(1);

        Assert.True(game.IsOver);
        Assert.Equal(1, game.MoveNumber);
    }

    [Fact]
    public void Play_WhenInvalid_DoesNotDrawReplacementCard()
    {
        var cards = OrderedCards();
        var game = new Game(cards);

        game.Play(1);

        Assert.Equal(3, game.DrawDeck.Count);
        Assert.Equal([cards[0], cards[2], cards[3], cards[4]], game.Hands[0].Slots.Select(slot => slot.Card));
    }

    [Fact]
    public void Play_WhenInvalid_DoesNotSwitchPlayers()
    {
        var game = new Game(OrderedCards());

        game.Play(1);

        Assert.Equal(0, game.CurrentPlayerIndex);
    }

    [Fact]
    public void Play_DrawingFinalDeckCardEndsGameOnThatMove()
    {
        var game = new Game(OrderedCards().Take(11));

        game.Play(0);

        Assert.True(game.IsOver);
        Assert.Equal(1, game.MoveNumber);
        Assert.Equal(0, game.CurrentPlayerIndex);
    }

    [Fact]
    public void Play_UnknownSuccessfulRankOneOnEmptyTableauIsRisky()
    {
        var game = new Game(OrderedCards());

        game.Play(0);

        Assert.Equal(1, game.RiskyPlayCount);
    }

    [Fact]
    public void Play_CardKnownToBeRankOneIsSafeOnEmptyTableau()
    {
        var game = new Game(OrderedCards());
        game.Hands[0].Slots[0].Knowledge.ApplyRankHint(1, isMatching: true);

        game.Play(0);

        Assert.Equal(0, game.RiskyPlayCount);
    }

    [Fact]
    public void Play_CardKnownExactlyAndPlayableIsSafe()
    {
        var game = new Game(OrderedCards());
        game.Hands[0].Slots[0].Knowledge.ApplyColorHint(CardColor.Red, isMatching: true);
        game.Hands[0].Slots[0].Knowledge.ApplyRankHint(1, isMatching: true);

        game.Play(0);

        Assert.Equal(0, game.RiskyPlayCount);
    }

    [Fact]
    public void Play_PartialKnowledgeWithPlayableAndUnplayablePossibilitiesIsRisky()
    {
        var cards = OrderedCards();
        var game = new Game(cards);
        game.Hands[0].Slots[0].Knowledge.ApplyColorHint(CardColor.Red, isMatching: true);
        game.Hands[0].Slots[0].Knowledge.ApplyRankHint(3, isMatching: false);
        game.Hands[0].Slots[0].Knowledge.ApplyRankHint(4, isMatching: false);
        game.Hands[0].Slots[0].Knowledge.ApplyRankHint(5, isMatching: false);

        game.Play(0);

        Assert.Equal(1, game.RiskyPlayCount);
    }

    [Fact]
    public void Play_RiskyCountIncrementsOnlyAfterSuccessfulRiskyPlays()
    {
        var game = new Game(OrderedCards());

        game.Play(0);

        Assert.Equal(1, game.RiskyPlayCount);
    }

    [Fact]
    public void Play_WhenInvalid_DoesNotIncrementRiskyPlayCount()
    {
        var game = new Game(OrderedCards());

        game.Play(1);

        Assert.Equal(0, game.RiskyPlayCount);
    }

    [Fact]
    public void TellColor_WhenValid_UpdatesMatchingAndNonMatchingKnowledge()
    {
        var game = new Game(MixedTargetHandCards());

        game.TellColor(CardColor.Blue, [0, 2]);

        Assert.Equal([CardColor.Blue], game.Hands[1].Slots[0].Knowledge.PossibleColors);
        Assert.DoesNotContain(CardColor.Blue, game.Hands[1].Slots[1].Knowledge.PossibleColors);
        Assert.Equal([CardColor.Blue], game.Hands[1].Slots[2].Knowledge.PossibleColors);
    }

    [Fact]
    public void TellRank_WhenValid_UpdatesMatchingAndNonMatchingKnowledge()
    {
        var game = new Game(MixedTargetHandCards());

        game.TellRank(1, [0, 3]);

        Assert.Equal([1], game.Hands[1].Slots[0].Knowledge.PossibleRanks);
        Assert.DoesNotContain(1, game.Hands[1].Slots[1].Knowledge.PossibleRanks);
        Assert.Equal([1], game.Hands[1].Slots[3].Knowledge.PossibleRanks);
    }

    [Fact]
    public void TellColor_WhenValid_IncrementsMoveNumberAndSwitchesPlayer()
    {
        var game = new Game(MixedTargetHandCards());

        game.TellColor(CardColor.Blue, [0, 2]);

        Assert.Equal(1, game.MoveNumber);
        Assert.Equal(1, game.CurrentPlayerIndex);
    }

    [Fact]
    public void TellRank_PlayerTwoCanGiveValidHintOnTheirTurn()
    {
        var game = new Game(MixedTargetHandCards());
        game.TellColor(CardColor.Blue, [0, 2]);

        game.TellRank(1, [0]);

        Assert.Equal([1], game.Hands[0].Slots[0].Knowledge.PossibleRanks);
        Assert.Equal(2, game.MoveNumber);
        Assert.Equal(0, game.CurrentPlayerIndex);
    }

    [Fact]
    public void TellColor_OmittingMatchingIndexEndsGame()
    {
        var game = new Game(MixedTargetHandCards());

        game.TellColor(CardColor.Blue, [0]);

        Assert.True(game.IsOver);
        Assert.Equal(1, game.MoveNumber);
    }

    [Fact]
    public void TellColor_IncludingNonMatchingIndexEndsGame()
    {
        var game = new Game(MixedTargetHandCards());

        game.TellColor(CardColor.Blue, [0, 1, 2]);

        Assert.True(game.IsOver);
        Assert.Equal(1, game.MoveNumber);
    }

    [Fact]
    public void TellColor_NamingAbsentColorEndsGame()
    {
        var game = new Game(MixedTargetHandCards());

        game.TellColor(CardColor.White, []);

        Assert.True(game.IsOver);
        Assert.Equal(1, game.MoveNumber);
    }

    [Fact]
    public void TellRank_NamingAbsentRankEndsGame()
    {
        var game = new Game(MixedTargetHandCards());

        game.TellRank(5, []);

        Assert.True(game.IsOver);
        Assert.Equal(1, game.MoveNumber);
    }

    [Fact]
    public void TellRank_DuplicateIndexEndsGame()
    {
        var game = new Game(MixedTargetHandCards());

        game.TellRank(1, [0, 0, 3]);

        Assert.True(game.IsOver);
        Assert.Equal(1, game.MoveNumber);
    }

    [Fact]
    public void TellRank_OutOfRangeIndexEndsGame()
    {
        var game = new Game(MixedTargetHandCards());

        game.TellRank(1, [0, 3, 5]);

        Assert.True(game.IsOver);
        Assert.Equal(1, game.MoveNumber);
    }

    [Fact]
    public void TellColor_WhenInvalid_DoesNotModifyKnowledge()
    {
        var game = new Game(MixedTargetHandCards());

        game.TellColor(CardColor.Blue, [0]);

        foreach (var slot in game.Hands[1].Slots)
        {
            Assert.Equal(Enum.GetValues<CardColor>(), slot.Knowledge.PossibleColors);
            Assert.Equal([1, 2, 3, 4, 5], slot.Knowledge.PossibleRanks);
        }
    }

    [Fact]
    public void TellColor_WhenInvalid_DoesNotSwitchPlayers()
    {
        var game = new Game(MixedTargetHandCards());

        game.TellColor(CardColor.Blue, [0]);

        Assert.Equal(0, game.CurrentPlayerIndex);
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

    private static List<Card> MixedTargetHandCards()
    {
        return
        [
            new Card(CardColor.Red, 1),
            new Card(CardColor.Red, 2),
            new Card(CardColor.Red, 3),
            new Card(CardColor.Red, 4),
            new Card(CardColor.Red, 5),
            new Card(CardColor.Blue, 1),
            new Card(CardColor.Green, 2),
            new Card(CardColor.Blue, 3),
            new Card(CardColor.Yellow, 1),
            new Card(CardColor.Green, 4),
            new Card(CardColor.White, 1),
        ];
    }
}
