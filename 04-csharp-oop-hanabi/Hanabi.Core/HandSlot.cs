namespace Hanabi.Core;

public sealed class HandSlot
{
    public HandSlot(Card card)
    {
        Card = card;
    }

    public Card Card { get; }

    public CardKnowledge Knowledge { get; } = new();
}
