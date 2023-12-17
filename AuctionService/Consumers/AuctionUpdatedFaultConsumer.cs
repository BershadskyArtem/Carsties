using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionUpdatedFaultConsumer : IConsumer<Fault<AuctionUpdated>>
{
    private readonly ILogger<AuctionUpdatedFaultConsumer> _logger;

    public AuctionUpdatedFaultConsumer(ILogger<AuctionUpdatedFaultConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<Fault<AuctionUpdated>> context)
    {
        _logger.LogError($"Fault while updating auction. Id: {context.Message.Message.Id}");
        return Task.CompletedTask;
    }
}