using AutoMapper;
using BusinessLogicLayer.Mappers;
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

        return services;
    }
}