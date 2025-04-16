namespace eCommerce.Core.Entities;

/// <summary>
/// Define the ApplicationUser class as entity model to store user details 
/// </summary>
public record ApplicationUser(
  Guid UserID,
  string? Email,
  string? Password,
  string? Name,
  string? Gender
  )
{
    //Parameterless constructor
    public ApplicationUser() : this(default, default, default, default, default)
    {
    }
}