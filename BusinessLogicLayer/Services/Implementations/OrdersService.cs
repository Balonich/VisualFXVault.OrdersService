using AutoMapper;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.Services.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using FluentValidation;
using MongoDB.Driver;

namespace BusinessLogicLayer.Services.Implementations;

public class OrdersService : IOrdersService
{
    private readonly IValidator<OrderAddRequestDto> _orderAddRequestValidator;
    private readonly IValidator<OrderItemAddRequestDto> _orderItemAddRequestValidator;
    private readonly IValidator<OrderUpdateRequestDto> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemUpdateRequestDto> _orderItemUpdateRequestValidator;
    private readonly UsersMicroserviceClient _usersMicroserviceClient;
    private readonly ProductsMicroserviceClient _productsMicroserviceClient;
    private readonly IMapper _mapper;
    private IOrdersRepository _ordersRepository;

    public OrdersService(IOrdersRepository ordersRepository,
        IMapper mapper,
        IValidator<OrderAddRequestDto> orderAddRequestValidator,
        IValidator<OrderItemAddRequestDto> orderItemAddRequestValidator,
        IValidator<OrderUpdateRequestDto> orderUpdateRequestValidator,
        IValidator<OrderItemUpdateRequestDto> orderItemUpdateRequestValidator,
        UsersMicroserviceClient usersMicroserviceClient,
        ProductsMicroserviceClient productsMicroserviceClient)
    {
        _orderAddRequestValidator = orderAddRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        _usersMicroserviceClient = usersMicroserviceClient;
        _productsMicroserviceClient = productsMicroserviceClient;
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

        await ValidateOrderRequestAsync(_orderAddRequestValidator, orderAddRequest);

        var productsFromOrder = await _productsMicroserviceClient.GetProductsByIdsAsync(
            orderAddRequest.OrderItems.Select(item => item.ProductId));

        await ValidateOrderItemsAsync(
            orderAddRequest.OrderItems,
            _orderItemAddRequestValidator,
            productsFromOrder,
            item => item.ProductId,
            product => product.ProductId);

        if (await _usersMicroserviceClient.GetUserByIdAsync(orderAddRequest.UserId) == null)
        {
            throw new ArgumentException($"User with ID {orderAddRequest.UserId} does not exist.");
        }

        var addedOrder = await _ordersRepository.CreateOrderAsync(InitializeOrderWithItems(orderAddRequest));

        if (addedOrder == null)
        {
            return null;
        }

        var addedOrderResponse = _mapper.Map<OrderResponseDto>(addedOrder);

        if (addedOrderResponse != null)
        {
            MapProductDetailsToOrderItems(
                addedOrderResponse.OrderItems,
                productsFromOrder,
                item => item.ProductId,
                product => product.ProductId);
        }

        return addedOrderResponse;
    }

    public async Task<OrderResponseDto?> UpdateOrderAsync(OrderUpdateRequestDto orderUpdateRequest)
    {
        if (orderUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(orderUpdateRequest));
        }

        await ValidateOrderRequestAsync(_orderUpdateRequestValidator, orderUpdateRequest);

        var productsFromOrder = await _productsMicroserviceClient.GetProductsByIdsAsync(
            orderUpdateRequest.OrderItems.Select(item => item.ProductId));

        await ValidateOrderItemsAsync(
            orderUpdateRequest.OrderItems,
            _orderItemUpdateRequestValidator,
            productsFromOrder,
            item => item.ProductId,
            product => product.ProductId
        );

        if (await _usersMicroserviceClient.GetUserByIdAsync(orderUpdateRequest.UserId) == null)
        {
            throw new ArgumentException($"User with ID {orderUpdateRequest.UserId} does not exist.");
        }

        var updatedOrder = await _ordersRepository.UpdateOrderAsync(InitializeOrderWithItems(orderUpdateRequest));

        if (updatedOrder == null)
        {
            return null;
        }

        var updatedOrderResponse = _mapper.Map<OrderResponseDto>(updatedOrder);

        if (updatedOrderResponse != null)
        {
            MapProductDetailsToOrderItems(
                updatedOrderResponse.OrderItems,
                productsFromOrder,
                item => item.ProductId,
                product => product.ProductId);
        }

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


    private async Task ValidateOrderRequestAsync<T>(IValidator<T> validator, T request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }
    }

    private async Task ValidateOrderItemsAsync<TItem, TProduct>(
        IEnumerable<TItem> orderItems,
        IValidator<TItem> validator,
        IEnumerable<TProduct> products,
        Func<TItem, Guid> getItemProductId,
        Func<TProduct, Guid> getProductId)
    {
        foreach (var orderItem in orderItems)
        {
            var productId = getItemProductId(orderItem);
            var product = products.FirstOrDefault(p => getProductId(p).Equals(productId));

            if (product == null)
            {
                throw new ArgumentException($"Product with ID {productId} does not exist.");
            }

            await ValidateOrderRequestAsync(validator, orderItem);
        }
    }

    private Order InitializeOrderWithItems<T>(T orderRequest)
    {
        var orderInput = _mapper.Map<Order>(orderRequest);

        foreach (var orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }
        orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);
        return orderInput;
    }

    private void MapProductDetailsToOrderItems<TItem, TProduct>(
        IEnumerable<TItem> orderItems,
        IEnumerable<TProduct> products,
        Func<TItem, Guid> getItemProductId,
        Func<TProduct, Guid> getProductId)
    {
        foreach (var orderItem in orderItems)
        {
            var productId = getItemProductId(orderItem);
            var product = products.FirstOrDefault(p => getProductId(p).Equals(productId));

            if (product == null)
            {
                continue;
            }

            _mapper.Map(product, orderItem);
        }
    }
}