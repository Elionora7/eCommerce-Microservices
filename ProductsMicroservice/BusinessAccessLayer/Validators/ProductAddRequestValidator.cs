using eCommerce.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.BusinessLogicLayer.Validators;

public class ProductAddRequestValidator : AbstractValidator<ProductAddRequest>
{
    public ProductAddRequestValidator()
    {
        // ProductName
        RuleFor(temp => temp.ProductName)
            .NotEmpty().WithMessage("Product Name is required!")
            .MaximumLength(50).WithMessage("Product Name cannot exceed 50 characters");

        // Category
        RuleFor(temp => temp.Category)
            .IsInEnum().WithMessage("Category should be in the list of the categories!");

        // UnitPrice
        RuleFor(temp => temp.UnitPrice)
            .InclusiveBetween(0, double.MaxValue)
            .WithMessage($"Unit Price should be between 0 to {double.MaxValue}")
            .When(temp => temp.UnitPrice.HasValue); // Only validate if value is provided

        // QuantityInStock
        RuleFor(temp => temp.QuantityInStock)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity in stock cannot be negative")
            .When(temp => temp.QuantityInStock.HasValue); // Only validate if value is provided

        // imgUrl (new field)
        RuleFor(temp => temp.imgUrl)
            .NotEmpty().WithMessage("Image URL is required")
            .MaximumLength(255).WithMessage("Image URL cannot exceed 255 characters")
            .Must(BeAValidUrl).When(temp => !string.IsNullOrEmpty(temp.imgUrl))
            .WithMessage("Invalid image URL format");
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}