using BusinessLogicLayer.MessageQueue.HostedServices;
using BusinessLogicLayer.MessageQueue.Implementations.Consumers;
using BusinessLogicLayer.MessageQueue.Interfaces.Consumers;
using BusinessLogicLayer.Services.Implementations;
using BusinessLogicLayer.Services.Interfaces;
using BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BusinessLogicLayer.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        services.AddScoped<IOrdersService, OrdersService>();

        services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();

        services.AddAutoMapper(config =>
        {
            config.AddMaps(Assembly.GetExecutingAssembly());
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = $"{Environment.GetEnvironmentVariable("REDIS_HOST")}:{Environment.GetEnvironmentVariable("REDIS_PORT")}";
        });

        services.AddTransient<IProductNameUpdatedConsumer, ProductNameUpdatedConsumer>();
        services.AddHostedService<ProductNameUpdatedHostedService>();

        services.AddTransient<IProductDeletedConsumer, ProductDeletedConsumer>();
        services.AddHostedService<ProductDeletedHostedService>();

        return services;
    }
}