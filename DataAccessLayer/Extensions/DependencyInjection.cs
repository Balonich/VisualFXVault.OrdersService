using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DataAccessLayer.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(
            configuration.GetSection(nameof(MongoDbSettings)));
        
        services.AddSingleton(serviceProvider => 
        {
            var options = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(options.ConnectionString);
        });

        services.AddScoped(serviceProvider => 
        {
            var options = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var client = serviceProvider.GetRequiredService<MongoClient>();
            return client.GetDatabase(options.DatabaseName);
        });

        services.AddScoped<IOrdersRepository, OrdersRepository>();

        return services;
    }
}