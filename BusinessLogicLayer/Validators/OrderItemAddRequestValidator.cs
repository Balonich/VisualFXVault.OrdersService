using BusinessLogicLayer.DTOs;
using FluentValidation;

namespace BusinessLogicLayer.Validators;

public class OrderItemAddRequestValidator : AbstractValidator<OrderItemAddRequestDto>
{
    public OrderItemAddRequestValidator()
    {
        RuleFor(temp => temp.ProductId)
            .NotEmpty().WithErrorCode("Product ID can't be blank");

        RuleFor(temp => temp.UnitPrice)
            .NotEmpty().WithErrorCode("Unit Price can't be blank")
            .GreaterThan(0).WithErrorCode("Unit Price can't be less than or equal to zero");

        RuleFor(temp => temp.Quantity)
            .NotEmpty().WithErrorCode("Quantity can't be blank")
            .GreaterThan(0).WithErrorCode("Quantity can't be less than or equal to zero");
    }
}