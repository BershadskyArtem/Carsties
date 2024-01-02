using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services
    .AddHttpClient<AuctionServiceHttpClient>()
    .AddPolicyHandler(GetPolicy());

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(conf =>
{
    //IRegistrationConfigurator
    conf.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    conf.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    conf.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", p =>
        {
            p.Username(builder.Configuration["RabbitMQ:Username"]);
            p.Password(builder.Configuration["RabbitMQ:Password"]);
        });
        
        cfg.ReceiveEndpoint("search-auction-updated", e =>
        {
            e.UseMessageRetry(r => 
                r.Incremental(5, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<AuctionUpdatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("search-auction-deleted", e =>
        {
            e.UseMessageRetry(r => 
                r.Incremental(5, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<AuctionDeletedConsumer>(context);
        });
        
        cfg.ReceiveEndpoint(
            "search-auction-created",
            e =>
            {
                e.UseMessageRetry(r =>
                    r.Interval(5, TimeSpan.FromSeconds(5)));
                e.ConfigureConsumer<AuctionCreatedConsumer>(context);
            });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await app.InitDb();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
});

app.UseAuthorization();

app.MapControllers();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
}