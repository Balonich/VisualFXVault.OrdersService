using BusinessLogicLayer.MessageQueue.Interfaces.Consumers;
using Microsoft.Extensions.Hosting;

namespace BusinessLogicLayer.MessageQueue.HostedServices;

public class ProductNameUpdatedHostedService : IHostedService
{
    private readonly IProductNameUpdatedConsumer _productNameUpdatedConsumer;

    public ProductNameUpdatedHostedService(IProductNameUpdatedConsumer productNameUpdatedConsumer)
    {
        _productNameUpdatedConsumer = productNameUpdatedConsumer;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _productNameUpdatedConsumer.ConsumeAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _productNameUpdatedConsumer.Dispose();

        return Task.CompletedTask;
    }
}