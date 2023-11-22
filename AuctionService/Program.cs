using AuctionService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AuctionDbContext>((isp, config) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    config.UseNpgsql(connectionString);
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//builder.Services.AddEndpointsApiExplorer();

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


