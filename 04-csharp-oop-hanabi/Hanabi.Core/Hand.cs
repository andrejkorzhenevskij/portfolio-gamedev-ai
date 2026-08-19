namespace Hanabi.Core;

public sealed class Hand
{
    private readonly List<Card> cards = [];

    public int Count => cards.Count;

    public IReadOnlyList<Card> Cards => cards;

    public void Add(Card card)
    {
        cards.Add(card);
    }

    public Card RemoveAt(int index)
    {
        var card = cards[index];
        cards.RemoveAt(index);
        return card;
    }
}
