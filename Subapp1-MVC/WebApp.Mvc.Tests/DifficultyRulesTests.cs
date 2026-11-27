using WebApp.Models;

namespace WebApp.Tests;

public class DifficultyRulesTests
{
    [Fact]
    [Trait("Category", "Negative")]
    public void Rejects_missing_blank_and_unknown_difficulties()
    {
        foreach (var difficulty in new[] { "", "   ", "Impossible" })
        {
            var normalizeError = Assert.Throws<ArgumentException>(() => DifficultyRules.Normalize(difficulty));
            var xpError = Assert.Throws<ArgumentException>(() => DifficultyRules.GetCompletionXp(difficulty));
            Assert.Equal("difficulty", normalizeError.ParamName);
            Assert.Equal("difficulty", xpError.ParamName);
        }

        Assert.Throws<ArgumentNullException>(() => DifficultyRules.Normalize(null!));
        Assert.Throws<ArgumentNullException>(() => DifficultyRules.GetCompletionXp(null!));
    }
}
