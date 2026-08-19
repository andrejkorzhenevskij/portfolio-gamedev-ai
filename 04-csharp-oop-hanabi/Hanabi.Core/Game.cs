namespace Hanabi.Core;

public sealed class Game
{
    public Game(IEnumerable<Card> cards)
    {
        var orderedCards = cards.ToList();

        if (orderedCards.Count < 11)
        {
            throw new ArgumentException("Game setup requires at least 11 cards.", nameof(cards));
        }

        var playerOneHand = new Hand();
        foreach (var card in orderedCards.Take(5))
        {
            playerOneHand.Add(card);
        }

        var playerTwoHand = new Hand();
        foreach (var card in orderedCards.Skip(5).Take(5))
        {
            playerTwoHand.Add(card);
        }

        Hands = [playerOneHand, playerTwoHand];
        DrawDeck = new Deck(orderedCards.Skip(10));
    }

    public IReadOnlyList<Hand> Hands { get; }

    public Deck DrawDeck { get; }

    public Tableau Tableau { get; } = new();

    public DiscardPile DiscardPile { get; } = new();

    public int CurrentPlayerIndex { get; private set; } = 0;

    public int MoveNumber { get; private set; } = 0;

    public int RiskyPlayCount { get; private set; } = 0;

    public bool IsOver { get; private set; } = false;

    public void Drop(int handIndex)
    {
        if (IsOver)
        {
            return;
        }

        var hand = Hands[CurrentPlayerIndex];
        if (!IsValidHandIndex(hand, handIndex))
        {
            EndInvalidAction();
            return;
        }

        var card = hand.RemoveAt(handIndex);
        DiscardPile.Add(card);

        CompleteSuccessfulAction(hand);
    }

    public void Play(int handIndex)
    {
        if (IsOver)
        {
            return;
        }

        var hand = Hands[CurrentPlayerIndex];
        if (!IsValidHandIndex(hand, handIndex))
        {
            EndInvalidAction();
            return;
        }

        var slot = hand.Slots[handIndex];
        var card = slot.Card;
        var isRisky = !Tableau.IsGuaranteedPlayable(slot.Knowledge);

        hand.RemoveAt(handIndex);

        if (!Tableau.CanPlay(card))
        {
            DiscardPile.Add(card);
            MoveNumber++;
            IsOver = true;
            return;
        }

        Tableau.Play(card);
        MoveNumber++;

        if (isRisky)
        {
            RiskyPlayCount++;
        }

        if (Tableau.Count == 25)
        {
            IsOver = true;
            return;
        }

        CompleteSuccessfulActionAfterMoveCounted(hand);
    }

    public void TellColor(CardColor color, IEnumerable<int> indices)
    {
        if (IsOver)
        {
            return;
        }

        var hand = Hands[1 - CurrentPlayerIndex];

        if (!HintMatches(hand, indices, card => card.Color == color))
        {
            FailHint();
            return;
        }

        hand.ApplyColorHint(color);
        CompleteHint();
    }

    public void TellRank(int rank, IEnumerable<int> indices)
    {
        if (IsOver)
        {
            return;
        }

        var hand = Hands[1 - CurrentPlayerIndex];

        if (!HintMatches(hand, indices, card => card.Rank == rank))
        {
            FailHint();
            return;
        }

        hand.ApplyRankHint(rank);
        CompleteHint();
    }

    private void CompleteSuccessfulAction(Hand hand)
    {
        MoveNumber++;
        CompleteSuccessfulActionAfterMoveCounted(hand);
    }

    private void CompleteSuccessfulActionAfterMoveCounted(Hand hand)
    {
        if (!DrawDeck.IsEmpty)
        {
            hand.Add(DrawDeck.Draw());
        }

        if (DrawDeck.IsEmpty)
        {
            IsOver = true;
            return;
        }

        CurrentPlayerIndex = 1 - CurrentPlayerIndex;
    }

    private void CompleteHint()
    {
        MoveNumber++;
        CurrentPlayerIndex = 1 - CurrentPlayerIndex;
    }

    private void FailHint()
    {
        MoveNumber++;
        IsOver = true;
    }

    private static bool IsValidHandIndex(Hand hand, int handIndex)
    {
        return handIndex >= 0 && handIndex < hand.Count;
    }

    private void EndInvalidAction()
    {
        MoveNumber++;
        IsOver = true;
    }

    private static bool HintMatches(Hand hand, IEnumerable<int> indices, Func<Card, bool> matches)
    {
        var supplied = indices.ToList();

        if (supplied.Distinct().Count() != supplied.Count ||
            supplied.Any(index => index < 0 || index >= hand.Count))
        {
            return false;
        }

        var actual = hand.Slots
            .Select((slot, index) => (slot, index))
            .Where(item => matches(item.slot.Card))
            .Select(item => item.index)
            .ToHashSet();

        return actual.Count > 0 && actual.SetEquals(supplied);
    }
}
