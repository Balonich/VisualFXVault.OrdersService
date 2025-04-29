namespace BusinessLogicLayer.DTOs;

public record OrderResponseDto(Guid OrderId, Guid UserId, decimal TotalBill, DateTime OrderDate, List<OrderItemResponseDto> OrderItems)
{
    public OrderResponseDto() : this(default, default, default, default, default)
    {
    }
}