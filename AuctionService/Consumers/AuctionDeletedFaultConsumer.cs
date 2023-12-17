using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionDeletedFaultConsumer : IConsumer<Fault<AuctionDeleted>>
{
    private readonly ILogger<AuctionDeletedFaultConsumer> _logger;

    public AuctionDeletedFaultConsumer(ILogger<AuctionDeletedFaultConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<Fault<AuctionDeleted>> context)
    {
        _logger.LogError($"Error while deleting entity with Id: {context.Message.Message.Id}");
        return Task.CompletedTask;
    }
}