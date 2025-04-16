using System.Xml.Linq;

namespace eCommerce.Core.DTO;

public record RegisterRequest(
  string? Email,
  string? Password,
  string? Name,
  GenderOptions Gender);