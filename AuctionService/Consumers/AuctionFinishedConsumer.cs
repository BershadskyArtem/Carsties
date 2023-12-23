using MassTransit;
using Microsoft.EntityFrameworkCore;
using Contracts;
using AuctionService.Data;
using AuctionService.Entities;

namespace AuctionService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    private readonly AuctionDbContext _auctionDbContext;

    public AuctionFinishedConsumer(AuctionDbContext auctionDbContext)
    {
        _auctionDbContext = auctionDbContext;
    }

    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        var message = context.Message;

        if (Guid.TryParse(message.AuctionId, out var auctionId))
        {
            throw new ArgumentException(nameof(message.AuctionId));
        }

        var finishedAuction = await _auctionDbContext.Auctions.FirstOrDefaultAsync(a => a.Id == auctionId);

        if (finishedAuction is null)
            throw new NullReferenceException(nameof(finishedAuction));

        if (message.ItemSold)
        {
            if(message.Winner is null)
                throw new NullReferenceException(nameof(message.Winner));
            
            if (message.Amount is null)
                throw new NullReferenceException(nameof(message.Amount));
            
            finishedAuction.SoldAmount = message.Amount;
            finishedAuction.Winner = message.Winner;
        }

        finishedAuction.Status = finishedAuction.SoldAmount > finishedAuction.ReservePrice
            ? Status.Finished
            : Status.ReserveNotMet;

        await _auctionDbContext.SaveChangesAsync();
    }
}