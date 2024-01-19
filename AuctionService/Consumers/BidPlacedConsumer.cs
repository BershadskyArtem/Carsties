using MassTransit;
using Microsoft.EntityFrameworkCore;
using AuctionService.Data;
using Contracts;

namespace AuctionService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly AuctionDbContext _auctionDbContext;

    public BidPlacedConsumer(AuctionDbContext auctionDbContext)
    {
        _auctionDbContext = auctionDbContext;
    }

    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        var message = context.Message;

        if (!Guid.TryParse(message.AuctionId, out var auctionId))
        {
            throw new ArgumentException(nameof(message.AuctionId));
        }

        var biddenAuction = await _auctionDbContext.Auctions.FirstOrDefaultAsync(a => a.Id == auctionId);

        if (biddenAuction is null)
        {
            throw new ArgumentNullException(nameof(biddenAuction));
        }

        // TODO: Remove hard coded string from contains method.
        if (biddenAuction.CurrentHighBid is null 
            || message.BidStatus.Contains("Accepted")
            && message.Amount > biddenAuction.CurrentHighBid)
        {
            biddenAuction.CurrentHighBid = message.Amount;

            await _auctionDbContext.SaveChangesAsync();
        }
    }
}