using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Services.Interfaces;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace APILayer.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrdersService _ordersService;

    public OrdersController(IOrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    [HttpGet]
    public async Task<IEnumerable<OrderResponseDto?>> GetOrders()
    {
        var orders = await _ordersService.GetOrdersAsync();

        return orders;
    }

    [HttpGet("search/order-id/{orderId}")]
    public async Task<OrderResponseDto?> GetOrderById(Guid orderId)
    {
        var order = await _ordersService.GetOrderByIdAsync(orderId);

        return order;
    }

    [HttpGet("search/product-id/{productId}")]
    public async Task<IEnumerable<OrderResponseDto?>> GetOrdersByProductId(Guid productId)
    {
        var filter = Builders<Order>.Filter.ElemMatch(o => o.OrderItems, oi => oi.ProductId == productId);
        var orders = await _ordersService.GetOrdersByCondition(filter);

        return orders;
    }

    [HttpGet("search/order-date/{orderDate}")]
    public async Task<IEnumerable<OrderResponseDto?>> GetOrdersByOrderDate(DateTime orderDate)
    {
        var filter = Builders<Order>.Filter.Eq(o => o.OrderDate, orderDate);
        var orders = await _ordersService.GetOrdersByCondition(filter);

        return orders;
    }

    [HttpPost]
    public async Task<IActionResult> Post(OrderAddRequestDto orderAddRequest)
    {
        if (orderAddRequest == null)
        {
            return BadRequest("Invalid order data");
        }

        var orderResponse = await _ordersService.CreateOrderAsync(orderAddRequest);

        if (orderResponse == null)
        {
            return Problem("Error in adding order");
        }


        return Created($"api/orders/search/orderid/{orderResponse?.OrderId}", orderResponse);
    }


    [HttpPut("{orderID}")]
    public async Task<IActionResult> Put(Guid orderID, OrderUpdateRequestDto orderUpdateRequest)
    {
        if (orderUpdateRequest == null)
        {
            return BadRequest("Invalid order data");
        }

        if (orderID != orderUpdateRequest.OrderId)
        {
            return BadRequest("OrderID in the URL doesn't match with the OrderID in the Request body");
        }

        var orderResponse = await _ordersService.UpdateOrderAsync(orderUpdateRequest);

        if (orderResponse == null)
        {
            return Problem("Error in updating order");
        }

        return Ok(orderResponse);
    }


    [HttpDelete("{orderId}")]
    public async Task<IActionResult> Delete(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            return BadRequest("Invalid order ID");
        }

        bool isDeleted = await _ordersService.DeleteOrderAsync(orderId);

        if (!isDeleted)
        {
            return Problem("Error in deleting order");
        }

        return Ok(isDeleted);
    }
}