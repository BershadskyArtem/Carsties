using MassTransit;
using MongoDB.Entities;
using SearchService.Models;
using Contracts;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        var message = context.Message;

        var biddenAuction = await DB.Find<Item>().OneAsync(message.AuctionId);

        if (biddenAuction is null)
        {
            throw new ArgumentNullException(nameof(biddenAuction));
        }

        if (message.BidStatus.Contains("Accepted") 
            && message.Amount > biddenAuction.CurrentHighBid)
        {
            biddenAuction.CurrentHighBid = message.Amount;
            await biddenAuction.SaveAsync();
        }
    }
}