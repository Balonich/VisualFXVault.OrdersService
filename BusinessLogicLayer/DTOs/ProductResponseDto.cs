using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogicLayer.DTOs;

public record ProductResponseDto(Guid ProductId, string ProductName, string Category, double? UnitPrice, int? QuantityInStock);