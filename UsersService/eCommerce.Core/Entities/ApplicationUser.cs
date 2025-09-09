namespace eCommerce.Core.Entities;

/// <summary>
/// Define the ApplicationUser class as entity model to store user details 
/// </summary>
public record ApplicationUser(
  Guid UserID,
  string? Email,
  string? Password,
  string? Name,
  string? Gender,
  string? RefreshToken,
  DateTime? RefreshTokenExpiryTime
  )
{
    //Parameterless constructor
    public ApplicationUser() : this(default, default, default, default, default,default,default)
    {
    }
}