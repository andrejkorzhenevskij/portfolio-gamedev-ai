namespace Hanabi.Core;

public sealed class Tableau
{
    private readonly int[] highestRanks = new int[Enum.GetValues<CardColor>().Length];

    public int Count => highestRanks.Sum();

    public bool CanPlay(Card card)
    {
        return card.Rank == highestRanks[(int)card.Color] + 1;
    }

    public bool IsGuaranteedPlayable(CardKnowledge knowledge)
    {
        return knowledge.PossibleColors.All(color =>
            knowledge.PossibleRanks.All(rank => CanPlay(new Card(color, rank))));
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
