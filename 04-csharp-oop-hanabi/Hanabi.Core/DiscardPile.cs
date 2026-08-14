namespace Hanabi.Core;

public sealed class DiscardPile
{
    private readonly List<Card> cards = [];

    public int Count => cards.Count;

    public IReadOnlyList<Card> Cards => cards;

    public void Add(Card card)
    {
        cards.Add(card);
    }
}
