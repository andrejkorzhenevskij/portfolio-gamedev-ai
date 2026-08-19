using Hanabi.Cli;
using Hanabi.Core;

namespace Hanabi.Tests;

public class CliRunnerTests
{
    [Fact]
    public void ParseDeck_ParsesCardNotation()
    {
        var cards = CliRunner.ParseDeck(["R1", "G2", "B3", "W4", "Y5"]);

        Assert.Equal(
            [
                new Card(CardColor.Red, 1),
                new Card(CardColor.Green, 2),
                new Card(CardColor.Blue, 3),
                new Card(CardColor.White, 4),
                new Card(CardColor.Yellow, 5),
            ],
            cards);
    }

    [Fact]
    public void Run_PlayOutputsExactResultWhenGameEnds()
    {
        var output = Run("""
            Start new game with deck R1 G2 B3 W4 Y5 R1 R1 B1 B2 W1 W2
            Play card 1
            """);

        Assert.Equal("1 0 0" + Environment.NewLine, output);
    }

    [Fact]
    public void Run_DropOutputsExactResultWhenGameEnds()
    {
        var output = Run("""
            Start new game with deck R1 G2 B3 W4 Y5 R1 R1 B1 B2 W1 W2
            Drop card 4
            """);

        Assert.Equal("1 0 0" + Environment.NewLine, output);
    }

    [Fact]
    public void Run_TellColorOutputsExactResultWhenGameEnds()
    {
        var output = Run("""
            Start new game with deck R1 G2 B3 W4 Y5 R1 R1 B1 B2 W1 W2
            Tell color Red for cards 0 1 2 3 4
            """);

        Assert.Equal("1 0 0" + Environment.NewLine, output);
    }

    [Fact]
    public void Run_TellRankOutputsExactResultWhenGameEnds()
    {
        var output = Run("""
            Start new game with deck R1 G2 B3 W4 Y5 R1 R1 B1 B2 W1 W2
            Tell rank 1 for cards 2 4
            """);

        Assert.Equal("1 0 0" + Environment.NewLine, output);
    }

    [Fact]
    public void Run_DoesNotOutputBeforeGameEnds()
    {
        var output = Run("""
            Start new game with deck R1 G2 B3 W4 Y5 R1 R1 B1 B2 W1 W2 W1
            Play card 0
            """);

        Assert.Equal("", output);
    }

    private static string Run(string input)
    {
        using var reader = new StringReader(input);
        using var writer = new StringWriter();

        CliRunner.Run(reader, writer);

        return writer.ToString();
    }
}
