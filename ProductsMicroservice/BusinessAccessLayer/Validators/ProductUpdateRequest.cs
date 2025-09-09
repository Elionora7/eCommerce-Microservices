using eCommerce.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.BusinessLogicLayer.Validators;

public class ProductUpdateRequestValidator : AbstractValidator<ProductUpdateRequest>
{
    public ProductUpdateRequestValidator()
    {
        //ProductID
        RuleFor(temp => temp.ProductID)
          .NotEmpty().WithMessage("Product ID is required!");

        //ProductName
        RuleFor(temp => temp.ProductName)
          .NotEmpty().WithMessage("Product Name is required!");

        //Category
        RuleFor(temp => temp.Category)
          .IsInEnum().WithMessage("Category should be included in the list of the categories!");

        //UnitPrice
        RuleFor(temp => temp.UnitPrice)
          .InclusiveBetween(0, double.MaxValue).WithMessage($"Unit Price should between 0 to {double.MaxValue}");

        //QuantityInStock
        RuleFor(temp => temp.QuantityInStock)
          .InclusiveBetween(0, int.MaxValue).WithMessage($"Quantity in Stock should between 0 to {int.MaxValue}");

        //imgUrl
        RuleFor(temp => temp.imgUrl)
          .NotEmpty().WithMessage("Product Image Url is required!");
    }
}