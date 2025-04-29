namespace BusinessLogicLayer.DTOs;

public record OrderItemAddRequestDto(Guid ProductId, decimal UnitPrice, int Quantity)
{
    public OrderItemAddRequestDto() : this(default, default, default)
    {
    }
}