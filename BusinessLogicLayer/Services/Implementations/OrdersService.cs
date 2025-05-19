using AutoMapper;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.Services.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;

namespace BusinessLogicLayer.Services.Implementations;

public class OrdersService : IOrdersService
{
    private readonly IValidator<OrderAddRequestDto> _orderAddRequestValidator;
    private readonly IValidator<OrderItemAddRequestDto> _orderItemAddRequestValidator;
    private readonly IValidator<OrderUpdateRequestDto> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemUpdateRequestDto> _orderItemUpdateRequestValidator;
    private readonly UsersMicroserviceClient _usersMicroserviceClient;
    private readonly IMapper _mapper;
    private IOrdersRepository _ordersRepository;

    public OrdersService(IOrdersRepository ordersRepository,
        IMapper mapper,
        IValidator<OrderAddRequestDto> orderAddRequestValidator,
        IValidator<OrderItemAddRequestDto> orderItemAddRequestValidator,
        IValidator<OrderUpdateRequestDto> orderUpdateRequestValidator,
        IValidator<OrderItemUpdateRequestDto> orderItemUpdateRequestValidator,
        UsersMicroserviceClient usersMicroserviceClient)
    {
        _orderAddRequestValidator = orderAddRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        _usersMicroserviceClient = usersMicroserviceClient;
        _mapper = mapper;
        _ordersRepository = ordersRepository;
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

    public async Task<OrderResponseDto?> CreateOrderAsync(OrderAddRequestDto orderAddRequest)
    {
        if (orderAddRequest == null)
        {
            throw new ArgumentNullException(nameof(orderAddRequest));
        }

        var orderAddRequestValidationResult = await _orderAddRequestValidator.ValidateAsync(orderAddRequest);
        if (!orderAddRequestValidationResult.IsValid)
        {
            var errors = string.Join(", ", orderAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        foreach (var orderItemAddRequest in orderAddRequest.OrderItems)
        {
            var orderItemAddRequestValidationResult = await _orderItemAddRequestValidator.ValidateAsync(orderItemAddRequest);

            if (!orderItemAddRequestValidationResult.IsValid)
            {
                var errors = string.Join(", ", orderItemAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }
        }

        if (await _usersMicroserviceClient.GetUserByIdAsync(orderAddRequest.UserId) == null)
        {
            throw new ArgumentException($"User with ID {orderAddRequest.UserId} does not exist.");
        }

        var orderInput = _mapper.Map<Order>(orderAddRequest);

        foreach (var orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }
        orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);


        var addedOrder = await _ordersRepository.CreateOrderAsync(orderInput);

        if (addedOrder == null)
        {
            return null;
        }

        var addedOrderResponse = _mapper.Map<OrderResponseDto>(addedOrder);

        return addedOrderResponse;
    }

    public async Task<OrderResponseDto?> UpdateOrderAsync(OrderUpdateRequestDto orderUpdateRequest)
    {
        if (orderUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(orderUpdateRequest));
        }

        var orderUpdateRequestValidationResult = await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);
        if (!orderUpdateRequestValidationResult.IsValid)
        {
            var errors = string.Join(", ", orderUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        foreach (var orderItemUpdateRequest in orderUpdateRequest.OrderItems)
        {
            var orderItemUpdateRequestValidationResult = await _orderItemUpdateRequestValidator.ValidateAsync(orderItemUpdateRequest);

            if (!orderItemUpdateRequestValidationResult.IsValid)
            {
                var errors = string.Join(", ", orderItemUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }
        }

        if (await _usersMicroserviceClient.GetUserByIdAsync(orderUpdateRequest.UserId) == null)
        {
            throw new ArgumentException($"User with ID {orderUpdateRequest.UserId} does not exist.");
        }

        var orderInput = _mapper.Map<Order>(orderUpdateRequest);

        foreach (var orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }
        orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);

        var updatedOrder = await _ordersRepository.UpdateOrderAsync(orderInput);

        if (updatedOrder == null)
        {
            return null;
        }

        var updatedOrderResponse = _mapper.Map<OrderResponseDto>(updatedOrder);

        return updatedOrderResponse;
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