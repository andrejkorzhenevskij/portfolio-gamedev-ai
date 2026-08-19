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
        var hand = Hands[CurrentPlayerIndex];
        var card = hand.RemoveAt(handIndex);
        DiscardPile.Add(card);

        CompleteSuccessfulAction(hand);
    }

    public void Play(int handIndex)
    {
        var hand = Hands[CurrentPlayerIndex];
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
        if (isRisky)
        {
            RiskyPlayCount++;
        }

        CompleteSuccessfulAction(hand);
    }

    private void CompleteSuccessfulAction(Hand hand)
    {
        if (!DrawDeck.IsEmpty)
        {
            hand.Add(DrawDeck.Draw());
        }

        MoveNumber++;

        if (DrawDeck.IsEmpty)
        {
            IsOver = true;
            return;
        }

        CurrentPlayerIndex = 1 - CurrentPlayerIndex;
    }
}
