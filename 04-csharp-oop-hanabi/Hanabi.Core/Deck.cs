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

    public static Deck CreateStandard()
    {
        var cards =
            from color in Enum.GetValues<CardColor>()
            from rank in Enumerable.Range(1, 5)
            from _ in Enumerable.Range(0, rank == 1 ? 3 : rank == 5 ? 1 : 2)
            select new Card(color, rank);

        return new Deck(cards);
    }

    public Card Draw()
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException("Cannot draw from an empty deck.");
        }

        return cards.Dequeue();
    }
}
