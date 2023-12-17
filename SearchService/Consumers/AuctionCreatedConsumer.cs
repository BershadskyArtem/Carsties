using AutoMapper;
using MassTransit;
using Contracts;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;
    
    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine($"-----> Consuming Auction Created Event: {context.Message.Id}");
     
        var item = _mapper.Map<Item>(context.Message);

        if (item.Model == "Foo")
            throw new ArgumentException(nameof(item.Model));
        
        await item.SaveAsync();
    }
}