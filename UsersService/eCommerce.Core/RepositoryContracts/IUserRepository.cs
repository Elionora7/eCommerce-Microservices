using eCommerce.Core.Entities;

namespace eCommerce.Core.RepositoryContracts;

/// <summary>
/// Contract to be implemented by UsersRepository that contains data access logic of Users data store
/// </summary>
public interface IUsersRepository
{
    /// <summary>
    /// Method to add a user and return the new user
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<ApplicationUser?> AddUser(ApplicationUser user);


    /// <summary>
    /// Method to retrieve existing user by email and password
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<ApplicationUser?> GetUserByEmailAndPassword(string? email, string? password);

    /// <summary>
    /// Return user data information based on userId
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>ApplicationUser Object</returns>
    Task<ApplicationUser?> GetUserByUserID(Guid? userId);

    /// <summary>
    /// Return user data information based on refreshToken
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns>ApplicationUser Object</returns>
    Task<ApplicationUser?> GetUserByRefreshToken(string refreshToken);

    Task<bool> UpdateUser(ApplicationUser user); 
}