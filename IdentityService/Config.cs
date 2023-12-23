using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("auctionApp", "Auction app full access scope."),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client()
            {
                ClientId = "postman",
                ClientName = "postman",
                AllowedScopes = new List<string>()
                {
                    "openid", "profile", "auctionApp"
                },
                RedirectUris = new List<string>()
                {
                    "https://www.lotsofms.com/oauth2/callback"
                },
                ClientSecrets = new List<Secret>()
                {
                    new Secret("NotASecret".Sha256())
                },
                AllowedGrantTypes = new List<string>()
                {
                    GrantType.ResourceOwnerPassword
                }
            },
            new Client()
            {
                ClientId = "nextApp",
                ClientName = "nextApp",
                ClientSecrets = new List<Secret>()
                {
                    new Secret("secret".Sha256())
                },
                AllowedScopes = new List<string>()
                {
                    "openid", "profile", "auctionApp"
                },
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                /*
                 * Since we can store secrets on the next serverside part we
                 * can you client credentials and code flow. But if we were
                 * using some non trusted runtime such as react native
                 * app then we could only use code flow. But since we are
                 * using trusted runtime we can disable PKCE requirement.
                 */
                RequirePkce = false,
                RedirectUris = new List<string>()
                {
                    "http://localhost:3000/api/auth/callback/id-server"
                },
                // Enabling refresh token support in here.
                AllowOfflineAccess = true,
                // Changing access token lifetime from 3600 seconds to month.
                AccessTokenLifetime = 3600 * 24 * 30
            }
        };
}