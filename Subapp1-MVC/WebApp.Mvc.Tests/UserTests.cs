using WebApp.Models;

namespace WebApp.Tests;

// User.IsOnline: the "online" indicator on the Users pages
public class UserTests
{
    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Type", "Unit")]
    public void User_seen_within_the_online_window_is_online()
    {
        var user = new User { LastSeenAt = DateTime.UtcNow.AddMinutes(-1) };

        Assert.True(user.IsOnline);
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Type", "Unit")]
    public void User_never_seen_or_seen_too_long_ago_is_not_online()
    {
        var neverSeen = new User { LastSeenAt = null };
        var seenLongAgo = new User { LastSeenAt = DateTime.UtcNow - User.OnlineWindow - TimeSpan.FromMinutes(1) };

        Assert.False(neverSeen.IsOnline);
        Assert.False(seenLongAgo.IsOnline);
    }
}
