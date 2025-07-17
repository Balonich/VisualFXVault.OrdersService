using System.Text;
using System.Text.Json;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.MessageQueue.Interfaces.Consumers;
using BusinessLogicLayer.MessageQueue.Messages;
using DnsClient.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BusinessLogicLayer.MessageQueue.Implementations.Consumers;

public class ProductNameUpdatedConsumer : IDisposable, IProductNameUpdatedConsumer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductNameUpdatedConsumer> _logger;
    private readonly IDistributedCache _distributedCache;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public ProductNameUpdatedConsumer(IConfiguration configuration,
        ILogger<ProductNameUpdatedConsumer> logger,
        IDistributedCache distributedCache)
    {
        _configuration = configuration;
        _logger = logger;
        _distributedCache = distributedCache;

        var hostName = _configuration["RABBITMQ_HOST"]!;
        var port = _configuration["RABBITMQ_PORT"]!;
        var userName = _configuration["RABBITMQ_USER"]!;
        var password = _configuration["RABBITMQ_PASSWORD"]!;

        var connectionFactory = new ConnectionFactory
        {
            HostName = hostName,
            Port = int.Parse(port),
            UserName = userName,
            Password = password
        };

        Task.Delay(5000).Wait(); // Ensure RabbitMQ is ready before connecting

        _connection = connectionFactory.CreateConnectionAsync().Result;

        _channel = _connection.CreateChannelAsync().Result;
    }

    public async Task ConsumeAsync()
    {
        var routingKey = "product.update.name";

        var exchangeName = _configuration["RABBITMQ_PRODUCTS_EXCHANGE"]!;
        await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct, durable: true);

        var queueName = "orders.product.update.name.queue";
        await _channel.QueueDeclareAsync(queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        await _channel.QueueBindAsync(queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            if (message != null)
            {
                var productNameUpdatedMessage = JsonSerializer.Deserialize<ProductNameUpdatedMessage>(message)!;

                var productFromCache = await _distributedCache.GetStringAsync($"product:{productNameUpdatedMessage.ProductId}");

                if (productFromCache != null)
                {
                    _logger.LogInformation($"Cache hit for product {productNameUpdatedMessage.ProductId}. Invalidating cache entry.");

                    var product = JsonSerializer.Deserialize<ProductResponseDto>(productFromCache)!;

                    await _distributedCache.SetStringAsync($"product:{product.ProductId}",
                        JsonSerializer.Serialize(product with
                        {
                            ProductName = productNameUpdatedMessage.NewProductName
                        }));
                }
                else
                {
                    _logger.LogInformation($"Cache miss for product {productNameUpdatedMessage.ProductId}. Skipping cache invalidation.");
                }

                _logger.LogInformation($"Product name updated message received: {productNameUpdatedMessage}");
            }
        };

        await _channel.BasicConsumeAsync(queue: queueName,
            autoAck: true,
            consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}