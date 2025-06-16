using APILayer.Middlewares;
using BusinessLogicLayer.Extensions;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.Interfaces.Policies;
using BusinessLogicLayer.Policies.Implementations;
using DataAccessLayer.Extensions;
using FluentValidation.AspNetCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();

builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer();

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200") // Replace with frontend URL
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<IMicroservicePolicies, MicroservicePolicies>();

builder.Services.AddHttpClient<UsersMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"{builder.Configuration["USERS_SERVICE_HOST"]}:{builder.Configuration["USERS_SERVICE_PORT"]}");
})
.AddPolicyHandler((serviceProvider, _) =>
    serviceProvider.GetRequiredService<IMicroservicePolicies>().GetExponentialRetryPolicy())
.AddPolicyHandler((serviceProvider, _) =>
    serviceProvider.GetRequiredService<IMicroservicePolicies>().GetCircuitBreakerPolicy());

builder.Services.AddHttpClient<ProductsMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"{builder.Configuration["PRODUCTS_SERVICE_HOST"]}:{builder.Configuration["PRODUCTS_SERVICE_PORT"]}");
})
.AddPolicyHandler((serviceProvider, _) =>
    serviceProvider.GetRequiredService<IMicroservicePolicies>().GetExponentialRetryPolicy())
.AddPolicyHandler((serviceProvider, _) =>
    serviceProvider.GetRequiredService<IMicroservicePolicies>().GetCircuitBreakerPolicy());

var app = builder.Build();

app.UseExceptionHandling();
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.MapOpenApi();

    app.MapScalarApiReference();
}

app.UseCors();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

