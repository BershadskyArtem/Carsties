using BiddingService.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services;

public class CheckAuctionFinished : BackgroundService
{
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting check for finished auctions.");

        stoppingToken.Register(() => { _logger.LogInformation("Stopping auction checking."); });

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuctions(stoppingToken);
            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task CheckAuctions(CancellationToken stoppingToken)
    {
        var finishedAuctions = await DB.Find<Auction>()
            .Match(a => a.AuctionEnds <= DateTime.UtcNow.ToUniversalTime())
            .Match(a => a.Finished == false)
            .ExecuteAsync(stoppingToken);

        if (finishedAuctions.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Found {Count} auctions that have completed.", finishedAuctions.Count);

        using var scope = _serviceProvider.CreateScope();
        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        foreach (var auction in finishedAuctions)
        {
            auction.Finished = true;
            await auction.SaveAsync(cancellation: stoppingToken);

            var winningBid = await DB.Find<Bid>()
                .Match(a => a.AuctionId == auction.ID)
                .Match(b => b.BidStatus == BidStatus.Accepted)
                .Sort(x => x.Descending(s => s.Amount))
                .ExecuteFirstAsync(stoppingToken);
            
            await endpoint.Publish(
                new AuctionFinished()
                {
                    Amount = winningBid?.Amount,
                    AuctionId = auction.ID!,
                    Seller = auction.Seller,
                    Winner = winningBid?.Bidder,
                    ItemSold = winningBid is not null
                }, 
                stoppingToken);
        }
    }
}