using Neurix.Helpers;
using Xunit;

namespace Neurix.Tests;

public class CardGridCssTests
{
    [Theory]
    [InlineData(0, "grid grid-cols-1 gap-8")]
    [InlineData(1, "grid grid-cols-1 gap-8")]
    [InlineData(2, "grid grid-cols-1 md:grid-cols-2 gap-8")]
    [InlineData(3, "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8")]
    [InlineData(4, "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8")]
    [InlineData(7, "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8")]
    public void ForItemCount_ReturnsColumnClassesMatchingCardCount(int count, string expectedClasses)
    {
        var actual = CardGridCss.ForItemCount(count);

        Assert.Equal(expectedClasses, actual);
    }
}
