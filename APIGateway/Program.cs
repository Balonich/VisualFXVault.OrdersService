using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services
    .AddOcelot(builder.Configuration)
    .AddPolly()
    .AddCacheManager(config =>
    {
        config.WithDictionaryHandle();
    });

var app = builder.Build();
await app.UseOcelot();

app.Run();
