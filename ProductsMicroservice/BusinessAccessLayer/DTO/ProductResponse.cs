using eCommerce.BusinessLogicLayer.DTO;

namespace eCommerce.BusinessLogicLayer.DTO;


public record ProductResponse(
    Guid ProductID,
    string ProductName,
    CategoryOptions Category,
    double? UnitPrice,
    int? QuantityInStock,
    string? imgUrl)
{ 
}