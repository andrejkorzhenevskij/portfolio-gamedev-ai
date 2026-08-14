namespace Hanabi.Core;

public sealed class Deck
{
    private readonly Queue<Card> cards;

    public Deck(IEnumerable<Card> cards)
    {
        this.cards = new Queue<Card>(cards);
    }

    public int Count => cards.Count;

    public bool IsEmpty => Count == 0;

    public Card Draw()
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException("Cannot draw from an empty deck.");
        }

        return cards.Dequeue();
    }
}
