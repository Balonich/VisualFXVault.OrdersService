namespace BusinessLogicLayer.DTOs;

public record ProductResponseDto(Guid ProductId, string ProductName, string Category, double? UnitPrice, int? QuantityInStock);