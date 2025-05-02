namespace BusinessLogicLayer.DTOs;

public record OrderItemUpdateRequestDto(Guid ProductId, decimal UnitPrice, int Quantity)
{
    public OrderItemUpdateRequestDto() : this(default, default, default)
    {
    }
}