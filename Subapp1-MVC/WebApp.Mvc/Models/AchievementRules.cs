namespace WebApp.Models;

public record AchievementProgress(string Title, string Description, int Progress, int Target);

// Achievements are derived from saved progress; no extra tables or duplicate awards.
public static class AchievementRules
{
    public static List<AchievementProgress> GetAchievements(int quizzesCompleted, int totalXp, IEnumerable<DateOnly> activityDates)
    {
        var longestStreak = 0;
        var currentStreak = 0;
        DateOnly? previous = null;
        foreach (var date in activityDates.Distinct().Order())
        {
            currentStreak = previous?.AddDays(1) == date ? currentStreak + 1 : 1;
            longestStreak = Math.Max(longestStreak, currentStreak);
            previous = date;
        }

        var level = totalXp / 100 + 1;
        AchievementProgress Badge(string title, string description, int progress, int target) =>
            new(title, description, Math.Min(progress, target), target);

        return
        [
            Badge("First quest", "Complete your first quiz.", quizzesCompleted, 1),
            Badge("On a roll", "Build a 5-day streak.", longestStreak, 5),
            Badge("Level up", "Reach level 2.", level, 2),
            Badge("Quest explorer", "Complete 3 different quizzes.", quizzesCompleted, 3),
            Badge("Dedicated learner", "Complete 10 different quizzes.", quizzesCompleted, 10),
            Badge("Rising star", "Reach level 3.", level, 3),
            Badge("Week warrior", "Build a 7-day streak.", longestStreak, 7)
        ];
    }
}
