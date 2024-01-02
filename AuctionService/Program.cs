using AuctionService.Consumers;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using AuctionService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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

    conf.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", p =>
        {
            p.Username(builder.Configuration["RabbitMQ:Username"]);
            p.Password(builder.Configuration["RabbitMQ:Password"]);
        });
        cfg.ConfigureEndpoints(context);
    });
});

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

var app = builder.Build();

app.UseAuthentication();
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