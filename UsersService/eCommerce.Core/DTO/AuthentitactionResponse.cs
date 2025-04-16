namespace eCommerce.Core.DTO;

public record AuthenticationResponse(
  Guid UserID,
  string? Email,
  string? Name,
  string? Gender,
  string? Token,
  bool Success
  )
{
    //Parametrless constructor
    public AuthenticationResponse():this(default,default,default, default, default, default)
{
}
}