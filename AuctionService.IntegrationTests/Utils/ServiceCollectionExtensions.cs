using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Utils;

public static class ServiceCollectionExtensions
{
    public static void RemoveDbContext<T>(this IServiceCollection services)
        where T : DbContext
    {
        var optionsType = typeof(DbContextOptions<>);
        var contextType = typeof(T);
        var optionsWithContextType = optionsType.MakeGenericType(contextType);
        
        var descriptor = services.SingleOrDefault(d => d.ServiceType == optionsWithContextType);
            
        if (descriptor is not null)
        {
            services.Remove(descriptor);    
        }
        else
        {
            throw new ArgumentException($"Cannot find DbContextOptions of {typeof(T)}");
        }
    }

    public static void EnsureDbCreated<T>(this IServiceCollection services, Action<T> seedDb)
        where T : DbContext
    {
        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<T>();
        db.Database.Migrate();
        seedDb(db);
    }
}