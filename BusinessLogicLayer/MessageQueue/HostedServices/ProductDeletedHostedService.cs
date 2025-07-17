using BusinessLogicLayer.MessageQueue.Interfaces.Consumers;
using Microsoft.Extensions.Hosting;

namespace BusinessLogicLayer.MessageQueue.HostedServices;

public class ProductDeletedHostedService : IHostedService
{
    private readonly IProductDeletedConsumer _productDeletedConsumer;

    public ProductDeletedHostedService(IProductDeletedConsumer productDeletedConsumer)
    {
        _productDeletedConsumer = productDeletedConsumer;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _productDeletedConsumer.ConsumeAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _productDeletedConsumer.Dispose();

        return Task.CompletedTask;
    }
}