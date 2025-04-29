using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Services.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using MongoDB.Driver;

namespace BusinessLogicLayer.Services.Implementations;

public class OrdersService : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository;

    public OrdersService(IOrdersRepository ordersRepository)
    {
        _ordersRepository = ordersRepository;
    }

    public Task<OrderResponseDto?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        throw new NotImplementedException();
    }

    public Task<OrderResponseDto?> GetOrderByIdAsync(Guid orderId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<OrderResponseDto?>> GetOrdersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<OrderResponseDto?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        throw new NotImplementedException();
    }

    public Task<OrderResponseDto?> CreateOrderAsync(OrderAddRequestDto order)
    {
        throw new NotImplementedException();
    }

    public Task<OrderResponseDto?> UpdateOrderAsync(OrderUpdateRequestDto order)
    {
        throw new NotImplementedException();
    }

    public Task<OrderResponseDto?> DeleteOrderAsync(Guid orderId)
    {
        throw new NotImplementedException();
    }
}