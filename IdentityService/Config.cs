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
            }
        };
}