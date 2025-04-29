using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IOrdersRepository
{
    public Task<IEnumerable<Order>> GetAllOrdersAsync();
    public Task<Order?> GetOrderByIdAsync(Guid orderId);
    public Task<IEnumerable<Order>> GetOrdersByCondition(FilterDefinition<Order> filter);
    public Task<Order?> CreateOrderAsync(Order order);
    public Task<Order?> UpdateOrderAsync(Order order);
    public Task<bool> DeleteOrderAsync(Guid orderId);
}