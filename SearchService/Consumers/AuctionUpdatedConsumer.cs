using MassTransit;
using MongoDB.Entities;
using SearchService.Models;
using Contracts;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        var updatedAuction = context.Message;

        // var item = await (await DB.Collection<Item>().FindAsync(x => x.ID == updatedAuction.Id)).FirstOrDefaultAsync();
        //
        // //We are unable to reconstruct new Auction from the data we have
        // if (item is null)
        //     throw new ArgumentException(nameof(item));
        //
        // item.Color = updatedAuction.Color ?? item.Color;
        // item.Make = updatedAuction.Make ?? item.Make;
        // item.Model = updatedAuction.Model ?? item.Model;
        // item.Mileage = updatedAuction.Mileage;
        // item.Year = updatedAuction.Year;
        //
        // await item.SaveAsync();
        
        /*
         * Also we can go like this
         * Update
         * .ModifyOnly(x => new {x.Color, x.Make,....}, item)
         * .ExecuteAsync()
         * 
         */
        

        var update = DB.Update<Item>()
            .MatchID(updatedAuction.Id)
            .Modify(b => b.Set(a => a.Mileage, updatedAuction.Mileage))
            .Modify(b => b.Set(a => a.Year, updatedAuction.Year));

        if (!string.IsNullOrEmpty(updatedAuction.Color))
        {
            update = update.Modify(b => b.Set(a => a.Color, updatedAuction.Color));
        }

        if (!string.IsNullOrEmpty(updatedAuction.Make))
        {
            update = update.Modify(b => b.Set(a => a.Make, updatedAuction.Make));
        }

        if (!string.IsNullOrEmpty(updatedAuction.Model))
        {
            update = update.Modify(b => b.Set(a => a.Model, updatedAuction.Model));
        }

        var result = await update.ExecuteAsync();

        if (result.ModifiedCount < 1)
            throw new ArgumentException("Unable to save updated values.");
    }
}