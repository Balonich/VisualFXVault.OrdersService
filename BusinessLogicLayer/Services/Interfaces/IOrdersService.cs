using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace BusinessLogicLayer.Services.Interfaces;

public interface IOrdersService
{
    Task<IEnumerable<OrderResponseDto?>> GetOrdersAsync();
    Task<IEnumerable<OrderResponseDto?>> GetOrdersByCondition(FilterDefinition<Order> filter);
    Task<OrderResponseDto?> GetOrderByCondition(FilterDefinition<Order> filter);
    Task<OrderResponseDto?> GetOrderByIdAsync(Guid orderId);
    Task<OrderResponseDto?> CreateOrderAsync(OrderAddRequestDto order);
    Task<OrderResponseDto?> UpdateOrderAsync(OrderUpdateRequestDto order);
    Task<bool> DeleteOrderAsync(Guid orderId);
}