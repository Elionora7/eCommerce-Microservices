using eCommerce.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.BusinessLogicLayer.Validators;

public class ProductAddRequestValidator : AbstractValidator<ProductAddRequest>
{
    public ProductAddRequestValidator()
    {
        //ProductName
        RuleFor(temp => temp.ProductName).NotEmpty()
            .WithMessage("Product Name is required!");

        //Category
        RuleFor(temp => temp.Category)
          .IsInEnum().WithMessage("Category should be in the list of the categories!");

        //UnitPrice
        RuleFor(temp => temp.UnitPrice)
          .InclusiveBetween(0, double.MaxValue).WithMessage($"Unit Price should between 0 to {double.MaxValue}");


    }
}