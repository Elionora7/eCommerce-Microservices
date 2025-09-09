

namespace eCommerce.Core.DTO;

public record UserDTO(Guid UserID, string? Email,string? Name, string Gender ,string? RefreshToken,
  DateTime? RefreshTokenExpiryTime);

