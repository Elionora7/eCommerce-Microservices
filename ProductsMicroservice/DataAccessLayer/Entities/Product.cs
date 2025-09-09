using System.ComponentModel.DataAnnotations;

namespace eCommerce.DataAccessLayer.Entities;

public class Product
{
    [Key]
    public Guid ProductID { get; set; }

    [Required]
    public required string ProductName { get; set; }

    [Required]
    public required string Category { get; set; }

    public double? UnitPrice { get; set; }
    public int? QuantityInStock { get; set; }
    public string? imgUrl { get; set; }
}