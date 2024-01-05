using System.Security.Claims;

namespace AuctionService.UnitTests.Utils;

public static class TestingHelpers
{
    public static string SameUser = "test";
    public static string DifferentUser = "NotTest";
    
    public static ClaimsPrincipal CreateClaimsPrincipal()
    {
        var claims = new List<Claim>()
        {
            new Claim("username", SameUser),
            new Claim(ClaimTypes.Name, SameUser)
        };

        var identity = new ClaimsIdentity(claims, "Testing");

        return new ClaimsPrincipal(identity);
    }
}