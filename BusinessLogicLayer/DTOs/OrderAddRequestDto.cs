namespace BusinessLogicLayer.DTOs;

public record OrderAddRequestDto(Guid UserId, DateTime OrderDate, List<OrderItemAddRequestDto> OrderItems)
{
    public OrderAddRequestDto() : this(default, default, default)
    {
    }
}