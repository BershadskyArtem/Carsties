using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
{
    private readonly ILogger<AuctionCreatedFaultConsumer> _logger;
    
    public AuctionCreatedFaultConsumer(ILogger<AuctionCreatedFaultConsumer> logger)
    {
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        //Console.WriteLine();

        _logger.LogInformation($"-----> Consuming Faulty creation. Id: {context.Message.Message.Id}");
        
        var exception = context.Message.Exceptions.First();

        if (exception.ExceptionType == nameof(System.ArgumentException))
        {
            //Or we can delete faulty shit from db.
            context.Message.Message.Model = "FooBar";
            await context.Publish(context.Message.Message);
        }

    }
}