using MassTransit;
using Contracts;

namespace SearchService.Consumers;

/// <summary>
/// Consumer created for testing and proving my theories about RabbitMQ. Not presented in the course. 
/// </summary>
public class TestingContractConsumer : IConsumer<TestingContract>
{
    public Task Consume(ConsumeContext<TestingContract> context)
    {
        Console.WriteLine($"Consuming testing message with id: {context.Message.Message}");
        return Task.CompletedTask;
    }
}