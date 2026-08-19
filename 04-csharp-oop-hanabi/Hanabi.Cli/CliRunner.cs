using Hanabi.Core;

namespace Hanabi.Cli;

public static class CliRunner
{
    public static void Run(TextReader input, TextWriter output)
    {
        Game? game = null;
        string? line;

        while ((line = input.ReadLine()) is not null)
        {
            var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                continue;
            }

            if (tokens is ["Start", "new", "game", "with", "deck", .. var deckTokens])
            {
                game = new Game(ParseDeck(deckTokens));
                continue;
            }

            if (game is null)
            {
                continue;
            }

            var wasOver = game.IsOver;
            Apply(game, tokens);

            if (!wasOver && game.IsOver)
            {
                output.WriteLine($"{game.MoveNumber} {game.Tableau.Count} {game.RiskyPlayCount}");
            }
        }
    }

    public static List<Card> ParseDeck(IEnumerable<string> tokens)
    {
        return tokens.Select(ParseCard).ToList();
    }

    private static void Apply(Game game, string[] tokens)
    {
        switch (tokens)
        {
            case ["Play", "card", var index]:
                game.Play(int.Parse(index));
                break;
            case ["Drop", "card", var index]:
                game.Drop(int.Parse(index));
                break;
            case ["Tell", "color", var color, "for", "cards", .. var indices]:
                game.TellColor(ParseColor(color), ParseIndices(indices));
                break;
            case ["Tell", "rank", var rank, "for", "cards", .. var indices]:
                game.TellRank(int.Parse(rank), ParseIndices(indices));
                break;
        }
    }

    private static Card ParseCard(string token)
    {
        return new Card(ParseColor(token[0]), int.Parse(token[1..]));
    }

    private static CardColor ParseColor(string color)
    {
        return color switch
        {
            "Red" or "R" => CardColor.Red,
            "Green" or "G" => CardColor.Green,
            "Blue" or "B" => CardColor.Blue,
            "Yellow" or "Y" => CardColor.Yellow,
            "White" or "W" => CardColor.White,
            _ => throw new ArgumentException("Unknown color.", nameof(color)),
        };
    }

    private static CardColor ParseColor(char color)
    {
        return ParseColor(color.ToString());
    }

    private static IEnumerable<int> ParseIndices(IEnumerable<string> tokens)
    {
        return tokens.Select(int.Parse);
    }
}
