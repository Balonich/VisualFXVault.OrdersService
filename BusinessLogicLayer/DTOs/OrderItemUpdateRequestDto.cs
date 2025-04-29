namespace BusinessLogicLayer.DTOs;

public record OrderItemUpdateRequestDto(Guid ProductID, decimal UnitPrice, int Quantity)
{
    public OrderItemUpdateRequestDto() : this(default, default, default)
    {
    }
}