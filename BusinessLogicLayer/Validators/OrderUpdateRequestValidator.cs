using BusinessLogicLayer.DTOs;
using FluentValidation;

namespace BusinessLogicLayer.Validators;

public class OrderUpdateRequestValidator : AbstractValidator<OrderUpdateRequestDto>
{
    public OrderUpdateRequestValidator()
    {
        RuleFor(temp => temp.OrderId)
            .NotEmpty().WithErrorCode("Order ID can't be blank");

        RuleFor(temp => temp.UserId)
            .NotEmpty().WithErrorCode("User ID can't be blank");

        RuleFor(temp => temp.OrderDate)
            .NotEmpty().WithErrorCode("Order Date can't be blank");

        RuleFor(temp => temp.OrderItems)
            .NotEmpty().WithErrorCode("Order Items can't be blank");
    }
}