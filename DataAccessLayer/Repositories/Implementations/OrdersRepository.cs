using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories.Implementations;

public class OrdersRepository : IOrdersRepository
{
    private readonly IMongoCollection<Order> _ordersCollection;

    public OrdersRepository(IMongoDatabase database)
    {
        _ordersCollection = database.GetCollection<Order>("Orders");
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        var filter = Builders<Order>.Filter.Empty;

        return await _ordersCollection.FindAsync(filter).Result.ToListAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.OrderId, orderId);

        return await _ordersCollection.FindAsync(filter).Result.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        return await _ordersCollection.FindAsync(filter).Result.ToListAsync();
    }

    public async Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        return await _ordersCollection.FindAsync(filter).Result.FirstOrDefaultAsync();
    }

    public async Task<Order?> CreateOrderAsync(Order order)
    {
        order.OrderId = Guid.NewGuid();
        order._id = order.OrderId;

        foreach (OrderItem orderItem in order.OrderItems)
        {
            orderItem._id = Guid.NewGuid();
        }

        await _ordersCollection.InsertOneAsync(order);
        return order;
    }

    public async Task<Order?> UpdateOrderAsync(Order order)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(x => x.OrderId, order.OrderId);

        Order? existingOrder = await _ordersCollection.FindAsync(filter).Result.FirstOrDefaultAsync();

        if (existingOrder == null)
        {
            return null;
        }

        order._id = existingOrder._id;

        ReplaceOneResult replaceOneResult = await _ordersCollection.ReplaceOneAsync(filter, order);

        return order;
    }

    public async Task<bool> DeleteOrderAsync(Guid orderId)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(x => x.OrderId, orderId);

        Order? existingOrder = await _ordersCollection.FindAsync(filter).Result.FirstOrDefaultAsync();

        if (existingOrder == null)
        {
            return false;
        }

        DeleteResult deleteResult = await _ordersCollection.DeleteOneAsync(filter);

        return deleteResult.DeletedCount > 0;
    }
}