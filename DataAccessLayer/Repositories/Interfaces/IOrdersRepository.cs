using System.Linq.Expressions;
using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IOrdersRepository
{
    public Task<IEnumerable<Order>> GetAllOrdersAsync();
    public Task<Order> GetOrderByIdAsync(Guid id);
    public Task<IEnumerable<Order>> GetOrdersByCondition(Expression<Func<Order, bool>> expression);
    public Task<Order> CreateOrderAsync(Order order);
    public Task<Order> UpdateOrderAsync(Order order);
    public Task<bool> DeleteOrderAsync(Guid id);
}