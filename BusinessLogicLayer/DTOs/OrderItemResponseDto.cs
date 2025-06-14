namespace BusinessLogicLayer.DTOs;

public record OrderItemResponseDto(Guid ProductId, string? ProductName, string? Category, decimal UnitPrice, int Quantity, decimal TotalPrice)
{
    public OrderItemResponseDto() : this(default, default, default, default, default, default)
    {
    }
}