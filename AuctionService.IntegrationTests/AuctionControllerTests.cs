using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Utils;

namespace AuctionService.IntegrationTests;

[Collection("Shared db")]
public class AuctionControllerTests : IAsyncLifetime
{
    /*
     * Since this is not a fixture
     * InitializeAsync and DisposeAsync are run after each test. 
     */

    private readonly CustomWebAppFactory _webAppFactory;
    private readonly HttpClient _httpClient;

    public AuctionControllerTests(CustomWebAppFactory webAppFactory)
    {
        _webAppFactory = webAppFactory;
        _httpClient = _webAppFactory.CreateClient();
    }

    [Fact]
    public async Task GetAuctions_ShouldReturnThreeAuctions()
    {
        var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("api/auctions");
        Assert.NotNull(response);
        Assert.Equal(10, response.Count);
    }
    
    [Fact]
    public async Task GetAuctionById_WithValidGuid_ShouldReturnAuction()
    {
        var auctionId = TestingHelper.GetTestAuctions().First().Id;
        var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{auctionId}");
        Assert.NotNull(response);
        Assert.Equal(auctionId, response.Id);
    }
    
    [Fact]
    public async Task GetAuctionById_WithInvalidGuid_ShouldReturnNotFound()
    {
        var response = await _httpClient.GetAsync($"api/auctions/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAuctionById_WithInvalidGuid_ShouldReturnBadRequest()
    {
        var response = await _httpClient.GetAsync("api/auctions/notaguid");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateAuction_WithNoAuth_ShouldReturnUnauthorized401()
    {
        var createAuctionDto = new CreateAuctionDto()
        {
            Color = "Some color"
        };
        
        var response = await _httpClient.PostAsJsonAsync("api/auctions", createAuctionDto);
        
        // We can do the following 
        // response.EnsureSuccessStatusCode();
        // or this.
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithAuth_ShouldReturnCreatedAuction201()
    {
        var createAuctionDto = TestingHelper.GetCreateAuctionDto();

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
        
        var response = await _httpClient.PostAsJsonAsync("api/auctions", createAuctionDto);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        
        Assert.NotNull(createdAuction);
        Assert.NotNull(createdAuction.Seller);
        Assert.Equal("bob", createdAuction.Seller);
    }
    
    [Fact]
    public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
    {
        // arrange
        var createAuctionDto = TestingHelper.GetCreateAuctionDto();

        // The following line for reserve price does not work. Because 0 is in fact valid Reserve price.
        // createAuctionDto.ReservePrice = default;
        createAuctionDto.Make = null;

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // act
        var result = await _httpClient.PostAsJsonAsync("api/auctions", createAuctionDto);
        
        // assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
    {
        // arrange
        var existingAuction = TestingHelper.GetTestAuctions().First();

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(existingAuction.Seller));

        var updateAuctionDto = new UpdateAuctionDto()
        {
            Color = "AS",
            Mileage = 15000,
            Make = "AS",
            Model = "AS",
            Year = 1000
        };

        // act
        var result = await _httpClient.PutAsJsonAsync($"api/auctions/{existingAuction.Id}", updateAuctionDto);

        // assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
    {
        // arrange
        var existingAuction = TestingHelper.GetTestAuctions().First();

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser($"{existingAuction.Seller}JUSTTOMESSWITHIT"));

        var updateAuctionDto = new UpdateAuctionDto()
        {
            Color = "AS",
            Mileage = 15000,
            Make = "AS",
            Model = "AS",
            Year = 1000
        };

        // act
        var result = await _httpClient.PutAsJsonAsync($"api/auctions/{existingAuction.Id}", updateAuctionDto);

        // assert
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        using var scope = _webAppFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        context.ReInitTestDb();
        return Task.CompletedTask;
    }
}