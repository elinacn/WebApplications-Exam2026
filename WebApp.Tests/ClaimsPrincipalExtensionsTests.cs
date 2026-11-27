using System.Security.Claims;
using WebApp.Extensions;

namespace WebApp.Tests;

// User.GetUserId(): how the app reads the signed-in user's ID from the login cookie's claims
public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Type", "Unit")]
    public void Reads_the_user_id_from_the_name_identifier_claim()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "42"),
            new Claim(ClaimTypes.Name, "Anna")
        ], "TestAuth"));

        Assert.Equal(42, principal.GetUserId());
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Type", "Unit")]
    public void Returns_no_id_for_anonymous_visitors_or_non_numeric_claims()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        var nameOnly = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "Anna")], "TestAuth"));
        var notANumber = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "anna")], "TestAuth"));

        Assert.Null(anonymous.GetUserId());
        Assert.Null(nameOnly.GetUserId());
        Assert.Null(notANumber.GetUserId());
    }
}
