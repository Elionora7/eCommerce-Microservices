using eCommerce.BusinessLogicLayer.DTO;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.BusinessLogicLayer.DTO;

public record ProductAddRequest(
    string ProductName,
    CategoryOptions Category,
    double? UnitPrice,
    int? QuantityInStock,
    string? imgUrl);