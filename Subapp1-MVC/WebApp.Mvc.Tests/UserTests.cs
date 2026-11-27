using WebApp.Models;

namespace WebApp.Tests;

public class UserTests
{
    [Fact]
    [Trait("Category", "Positive")]
    public void Recently_seen_user_is_online()
    {
        var user = new User { LastSeenAt = DateTime.UtcNow.AddMinutes(-1) };

        Assert.True(user.IsOnline);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Never_seen_or_inactive_user_is_not_online()
    {
        var neverSeen = new User { LastSeenAt = null };
        var inactive = new User { LastSeenAt = DateTime.UtcNow - User.OnlineWindow - TimeSpan.FromMinutes(1) };

        Assert.False(neverSeen.IsOnline);
        Assert.False(inactive.IsOnline);
    }
}
