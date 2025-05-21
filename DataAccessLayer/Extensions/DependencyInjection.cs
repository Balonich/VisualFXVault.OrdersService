using DataAccessLayer.Repositories.Implementations;
using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            var connectionStringTemplate = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value.ConnectionString;

            var connectionString = connectionStringTemplate
                .Replace("$MONGO_HOST", Environment.GetEnvironmentVariable("MONGO_HOST"))
                .Replace("$MONGO_PORT", Environment.GetEnvironmentVariable("MONGO_PORT"));

            return new MongoClient(connectionString);
        });

        services.AddScoped(serviceProvider =>
        {
            var databaseName = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value.DatabaseName
                .Replace("$MONGO_DB", Environment.GetEnvironmentVariable("MONGO_DB"));

            var client = serviceProvider.GetRequiredService<MongoClient>();

            return client.GetDatabase(databaseName);
        });

        services.AddScoped<IOrdersRepository, OrdersRepository>();

        return services;
    }
}