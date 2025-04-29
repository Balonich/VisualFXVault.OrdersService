namespace BusinessLogicLayer.DTOs;

public record OrderAddRequestDto(Guid UserID, DateTime OrderDate, List<OrderItemAddRequestDto> OrderItems)
{
    public OrderAddRequestDto() : this(default, default, default)
    {
    }
}