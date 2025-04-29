using System.Linq.Expressions;
using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories.Interfaces;

public class OrdersRepository : IOrdersRepository
{
    private readonly IMongoCollection<Order> _ordersCollection;

    public OrdersRepository(IMongoDatabase database)
    {
        _ordersCollection = database.GetCollection<Order>("Orders");
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await _ordersCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Order> GetOrderByIdAsync(Guid id)
    {
        return await  _ordersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByCondition(Expression<Func<Order, bool>> expression)
    {
        return await _ordersCollection.Find(expression).ToListAsync();
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        await _ordersCollection.InsertOneAsync(order);

        return order;
    }

    public async Task<Order> UpdateOrderAsync(Order order)
    {
        await _ordersCollection.ReplaceOneAsync(x => x.Id == order.Id, order);

        return order;
    }

    public async Task<bool> DeleteOrderAsync(Guid id)
    {
        var deleteResult = await _ordersCollection.DeleteOneAsync(x => x.Id == id);

        return deleteResult.DeletedCount > 0;
    }
}