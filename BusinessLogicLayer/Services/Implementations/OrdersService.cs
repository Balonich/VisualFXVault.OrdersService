using AutoMapper;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Services.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using MongoDB.Driver;

namespace BusinessLogicLayer.Services.Implementations;

public class OrdersService : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IMapper _mapper;

    public OrdersService(IOrdersRepository ordersRepository, IMapper mapper)
    {
        _ordersRepository = ordersRepository;
        _mapper = mapper;
    }

    public async Task<OrderResponseDto?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        Order? order = await _ordersRepository.GetOrderByCondition(filter);

        if (order == null)
        {
            return null;
        }

        return _mapper.Map<OrderResponseDto>(order);
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(Guid orderId)
    {
        Order? order = await _ordersRepository.GetOrderByIdAsync(orderId);

        if (order == null)
        {
            return null;
        }

        return _mapper.Map<OrderResponseDto>(order);
    }

    public async Task<IEnumerable<OrderResponseDto?>> GetOrdersAsync()
    {
        IEnumerable<Order> orders = await _ordersRepository.GetAllOrdersAsync();

        return _mapper.Map<IEnumerable<OrderResponseDto?>>(orders);
    }

    public async Task<IEnumerable<OrderResponseDto?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        IEnumerable<Order> orders = await _ordersRepository.GetOrdersByCondition(filter);

        return _mapper.Map<IEnumerable<OrderResponseDto?>>(orders);
    }

    public async Task<OrderResponseDto?> CreateOrderAsync(OrderAddRequestDto orderDto)
    {
        var order = _mapper.Map<Order>(orderDto);

        foreach (var item in order.OrderItems)
        {
            item.TotalPrice = item.UnitPrice * item.Quantity;
        }

        order.TotalBill = order.OrderItems.Sum(item => item.TotalPrice);

        var createdOrder = await _ordersRepository.CreateOrderAsync(order);

        if (createdOrder == null)
        {
            return null;
        }

        return _mapper.Map<OrderResponseDto>(createdOrder);
    }

    public async Task<OrderResponseDto?> UpdateOrderAsync(OrderUpdateRequestDto orderDto)
    {
        var existingOrder = await _ordersRepository.GetOrderByIdAsync(orderDto.OrderId);

        if (existingOrder == null)
        {
            return null;
        }

        var order = _mapper.Map<Order>(orderDto);

        foreach (var item in order.OrderItems)
        {
            item.TotalPrice = item.UnitPrice * item.Quantity;
        }

        order.TotalBill = order.OrderItems.Sum(item => item.TotalPrice);

        var updatedOrder = await _ordersRepository.UpdateOrderAsync(order);

        if (updatedOrder == null)
        {
            return null;
        }

        return _mapper.Map<OrderResponseDto>(updatedOrder);
    }

    public async Task<bool> DeleteOrderAsync(Guid orderId)
    {
        var order = await _ordersRepository.GetOrderByIdAsync(orderId);

        if (order == null)
        {
            return false;
        }

        var isDeleted = await _ordersRepository.DeleteOrderAsync(orderId);

        return isDeleted;
    }
}