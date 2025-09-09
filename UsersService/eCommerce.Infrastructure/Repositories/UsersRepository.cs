using Dapper;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;
using eCommerce.Core.RepositoryContracts;
using eCommerce.Infrastructure.DbContext;

namespace eCommerce.Infrastructure.Repositories;

internal class UsersRepository : IUsersRepository
{
    private readonly DapperDbContext _dbContext;

    public UsersRepository(DapperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApplicationUser?> AddUser(ApplicationUser user)
    {
        string query = @"
            INSERT INTO public.""Users""(
                ""UserID"", ""Email"", ""Name"", ""Gender"", ""Password"", ""RefreshToken"", ""RefreshTokenExpiryTime""
            ) VALUES (
                @UserID, @Email, @Name, @Gender, @Password, @RefreshToken, @RefreshTokenExpiryTime
            )";

        int rowCountAffected = await _dbContext.DbConnection.ExecuteAsync(query, user);

        return rowCountAffected > 0 ? user : null;
    }

    public async Task<ApplicationUser?> GetUserByEmailAndPassword(string? email, string? password)
    {
        string query = "SELECT * FROM public.\"Users\" WHERE \"Email\"=@Email AND \"Password\"=@Password";
        var parameters = new { Email = email, Password = password };

        return await _dbContext.DbConnection.QueryFirstOrDefaultAsync<ApplicationUser>(query, parameters);
    }

    public async Task<ApplicationUser?> GetUserByUserID(Guid? userId)
    {
        string query = "SELECT * FROM public.\"Users\" WHERE \"UserID\" = @UserID";
        var parameters = new { UserID = userId };

        return await _dbContext.DbConnection.QueryFirstOrDefaultAsync<ApplicationUser>(query, parameters);
    }
    public async Task<ApplicationUser?> GetUserByRefreshToken(string refreshToken)
    {
        string query = @"SELECT * FROM public.""Users"" 
                     WHERE ""RefreshToken"" = @RefreshToken";
        var parameters = new { RefreshToken = refreshToken };

        return await _dbContext.DbConnection
            .QueryFirstOrDefaultAsync<ApplicationUser>(query, parameters);
    }

    public async Task<bool> UpdateUser(ApplicationUser user)
    {
        string query = @"
        UPDATE public.""Users""
        SET ""Email"" = @Email,
            ""Name"" = @Name,
            ""Gender"" = @Gender,
            ""Password"" = @Password,
            ""RefreshToken"" = @RefreshToken,
            ""RefreshTokenExpiryTime"" = @RefreshTokenExpiryTime
        WHERE ""UserID"" = @UserID";

        int rows = await _dbContext.DbConnection.ExecuteAsync(query, user);
        return rows > 0;
    }


}
