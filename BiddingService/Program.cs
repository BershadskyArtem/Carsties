using MassTransit;
using MongoDB.Driver;
using MongoDB.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BiddingService.Consumers;
using BiddingService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false;
#pragma warning disable CA5404
        options.TokenValidationParameters.ValidateAudience = false;
#pragma warning restore CA5404
        options.TokenValidationParameters.NameClaimType = "username";
    });

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(busConfig =>
{
    busConfig.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    busConfig.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));

    busConfig.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", host =>
        {
            host.Password(builder.Configuration["RabbitMQ:Password"]);
            host.Username(builder.Configuration["RabbitMQ:Username"]);
        });    
        
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHostedService<CheckAuctionFinished>();

builder.Services.AddScoped<GrpcAuctionClient>();

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

await DB.InitAsync("BidDb",
    MongoClientSettings.FromConnectionString(
        builder.Configuration.GetConnectionString("BidDbConnection")));

app.Run();