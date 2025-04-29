namespace BusinessLogicLayer.DTOs;

public record OrderUpdateRequestDto(Guid OrderId, Guid UserId, DateTime OrderDate, List<OrderItemUpdateRequestDto> OrderItems)
{
    public OrderUpdateRequestDto() : this(default, default, default, default)
    {
    }
}