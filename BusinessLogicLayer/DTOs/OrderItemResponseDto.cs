namespace BusinessLogicLayer.DTOs;

public record OrderItemResponseDto(Guid ProductId, decimal UnitPrice, int Quantity, decimal TotalPrice)
{
    public OrderItemResponseDto() : this(default, default, default, default)
    {
    }
}