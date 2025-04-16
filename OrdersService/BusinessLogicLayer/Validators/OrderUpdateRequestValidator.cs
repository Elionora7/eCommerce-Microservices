using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;

public class OrderUpdateRequestValidator : AbstractValidator<OrderUpdateRequest>
{
    public OrderUpdateRequestValidator()
    {
        //OrderID
        RuleFor(temp => temp.OrderID)
          .NotEmpty().WithErrorCode("Order ID is required!");

        //UserID
        RuleFor(temp => temp.UserID)
          .NotEmpty().WithErrorCode("User ID is required!");

        //OrderDate
        RuleFor(temp => temp.OrderDate)
          .NotEmpty().WithErrorCode("Order Date is required!");

        //OrderItems
        RuleFor(temp => temp.OrderItems)
          .NotEmpty().WithErrorCode("Order Items can't be empty!");
    }
}