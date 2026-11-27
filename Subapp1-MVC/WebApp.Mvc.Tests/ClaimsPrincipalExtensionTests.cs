using System.Security.Claims;
using WebApp.Extensions;

namespace WebApp.Tests;

public class ClaimsPrincipalExtensionTests
{
    [Fact]
    [Trait("Category", "Positive")]
    public void Returns_the_signed_in_users_numeric_id()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "42"), new Claim(ClaimTypes.Name, "Anna")], "Test"));

        Assert.Equal(42, principal.GetUserId());
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Returns_no_id_for_anonymous_missing_or_non_numeric_claims()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        var missingId = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "Anna")], "Test"));
        var invalidId = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "not-a-number")], "Test"));

        Assert.Null(anonymous.GetUserId());
        Assert.Null(missingId.GetUserId());
        Assert.Null(invalidId.GetUserId());
    }
}
