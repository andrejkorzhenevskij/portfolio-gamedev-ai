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

    public int CurrentPlayerIndex { get; } = 0;

    public int MoveNumber { get; } = 0;

    public int RiskyPlayCount { get; } = 0;

    public bool IsOver { get; } = false;
}
