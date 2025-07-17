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

public class ProductDeletedConsumer : IDisposable, IProductDeletedConsumer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductDeletedConsumer> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly IDistributedCache _distributedCache;

    public ProductDeletedConsumer(IConfiguration configuration, ILogger<ProductDeletedConsumer> logger, IDistributedCache distributedCache)
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
        var routingKey = "product.delete";

        var exchangeName = _configuration["RABBITMQ_PRODUCTS_EXCHANGE"]!;
        await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct, durable: true);

        var queueName = "orders.product.delete.queue";
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
                var productDeletedMessage = JsonSerializer.Deserialize<ProductDeletedMessage>(message)!;
                
                await _distributedCache.RemoveAsync($"product:{productDeletedMessage.ProductId}");

                _logger.LogInformation($"Product deleted message received: {productDeletedMessage}");
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