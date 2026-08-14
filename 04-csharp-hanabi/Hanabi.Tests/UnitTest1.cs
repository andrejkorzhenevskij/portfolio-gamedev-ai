using Hanabi.Core;

namespace Hanabi.Tests;

public class CardTests
{
    [Fact]
    public void Cards_WithSameValues_AreEqual()
    {
        var first = new Card(CardColor.Red, 1);
        var second = new Card(CardColor.Red, 1);

        Assert.Equal(first, second);
    }
}