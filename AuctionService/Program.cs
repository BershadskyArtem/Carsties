using AuctionService.Consumers;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using AuctionService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AuctionDbContext>((isp, config) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    config.UseNpgsql(connectionString);
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(conf =>
{
    conf.AddEntityFrameworkOutbox<AuctionDbContext>(opt =>
    {
        opt.QueryDelay = TimeSpan.FromSeconds(10);
        opt.UsePostgres();
        opt.UseBusOutbox();
    });

    conf.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

    conf.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    conf.UsingRabbitMq((context, cfg) => { cfg.ConfigureEndpoints(context); });
});


var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

try
{
    app.InitDb();
}
catch (Exception e)
{
    Console.WriteLine(e);
}


app.Run();