using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        var message = context.Message;
        
        var finishedAuction = await DB.Find<Item>().OneAsync(message.AuctionId);

        if (finishedAuction is null)
            throw new NullReferenceException(nameof(finishedAuction));

        if (message.ItemSold)
        {
            if (message.Winner is null)
                throw new NullReferenceException(nameof(message.Winner));

            if (message.Amount is null)
                throw new NullReferenceException(nameof(message.Amount));
            
            finishedAuction.Winner = message.Winner;
            finishedAuction.SoldAmount = (int)message.Amount;
        }

        //TODO:Replace hardcoded string value.
        finishedAuction.Status = "Finished";

        await finishedAuction.SaveAsync();
    }
}