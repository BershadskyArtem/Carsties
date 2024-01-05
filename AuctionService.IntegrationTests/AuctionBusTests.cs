using System.Net;
using System.Net.Http.Json;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Contracts;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Utils;

namespace AuctionService.IntegrationTests;

[Collection("Shared db")]
public class AuctionBusTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _webAppFactory;
    private readonly HttpClient _httpClient;
    private readonly ITestHarness _testHarness;

    public AuctionBusTests(CustomWebAppFactory webAppFactory)
    {
        _webAppFactory = webAppFactory;
        _httpClient = _webAppFactory.CreateClient();
        _testHarness = _webAppFactory.Services.GetTestHarness();
    }

    [Fact]
    public async Task CreateAuction_WithValidObject_ShouldPublishAuctionCreated()
    {
        var createAuctionDto = TestingHelper.GetCreateAuctionDto();

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
        
        var response = await _httpClient.PostAsJsonAsync("api/auctions", createAuctionDto);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();

        Assert.NotNull(createdAuction);
        Assert.NotNull(createdAuction.Seller);
        Assert.Equal("bob", createdAuction.Seller);
        Assert.True(await _testHarness.Published.Any<AuctionCreated>());
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