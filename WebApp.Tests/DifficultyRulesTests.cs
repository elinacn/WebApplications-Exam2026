using WebApp.Models;

namespace WebApp.Tests;

// DifficultyRules: quest difficulty names and the XP they award.
// Valid and legacy names are covered by DifficultyXpTests.
public class DifficultyRulesTests
{
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Type", "Unit")]
    public void Unknown_or_empty_difficulties_are_rejected()
    {
        Assert.Throws<ArgumentException>(() => DifficultyRules.Normalize("legendary"));
        Assert.Throws<ArgumentException>(() => DifficultyRules.Normalize(""));
        Assert.Throws<ArgumentException>(() => DifficultyRules.Normalize("   "));
        Assert.Throws<ArgumentException>(() => DifficultyRules.GetCompletionXp("legendary"));
    }
}
