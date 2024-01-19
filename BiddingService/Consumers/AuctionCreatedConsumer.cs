using BiddingService.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        var message = context.Message;

        var auction = new Auction()
        {
            ID = message.Id.ToString(),
            AuctionEnds = message.AuctionEnd,
            Finished = false,
            ReservePrice = message.ReservePrice,
            Seller = message.Seller
        };

        await auction.SaveAsync();
    }
}