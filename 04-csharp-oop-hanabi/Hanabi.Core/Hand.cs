namespace Hanabi.Core;

public sealed class Hand
{
    private readonly List<HandSlot> slots = [];

    public int Count => slots.Count;

    public IReadOnlyList<HandSlot> Slots => slots;

    public void Add(Card card)
    {
        slots.Add(new HandSlot(card));
    }

    public Card RemoveAt(int index)
    {
        var slot = slots[index];
        slots.RemoveAt(index);
        return slot.Card;
    }
}
