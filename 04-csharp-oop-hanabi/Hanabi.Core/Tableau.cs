namespace Hanabi.Core;

public sealed class Tableau
{
    private readonly int[] highestRanks = new int[Enum.GetValues<CardColor>().Length];

    public int Count => highestRanks.Sum();

    public bool CanPlay(Card card)
    {
        return card.Rank == highestRanks[(int)card.Color] + 1;
    }

    public void Play(Card card)
    {
        if (!CanPlay(card))
        {
            throw new InvalidOperationException("Cannot play card on tableau.");
        }

        highestRanks[(int)card.Color] = card.Rank;
    }
}
