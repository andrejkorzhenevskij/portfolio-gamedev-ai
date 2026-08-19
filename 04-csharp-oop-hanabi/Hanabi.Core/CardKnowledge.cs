namespace Hanabi.Core;

public sealed class CardKnowledge
{
    private readonly List<CardColor> possibleColors = Enum.GetValues<CardColor>().ToList();
    private readonly List<int> possibleRanks = [1, 2, 3, 4, 5];

    public IReadOnlyList<CardColor> PossibleColors => possibleColors;

    public IReadOnlyList<int> PossibleRanks => possibleRanks;

    public void ApplyColorHint(CardColor color, bool isMatching)
    {
        if (isMatching)
        {
            possibleColors.RemoveAll(possibleColor => possibleColor != color);
            return;
        }

        possibleColors.Remove(color);
    }

    public void ApplyRankHint(int rank, bool isMatching)
    {
        if (isMatching)
        {
            possibleRanks.RemoveAll(possibleRank => possibleRank != rank);
            return;
        }

        possibleRanks.Remove(rank);
    }
}
