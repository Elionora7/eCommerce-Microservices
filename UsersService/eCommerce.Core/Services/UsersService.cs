using AutoMapper;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;
using eCommerce.Core.RepositoryContracts;
using eCommerce.Core.ServiceContracts;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace eCommerce.Core.Services;

public class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    public UsersService(IUsersRepository usersRepository,
                        IMapper mapper,
                        IJwtService jwtService)
    {
        _usersRepository = usersRepository;
        _mapper = mapper;
        _jwtService = jwtService;
    }

    public async Task<UserDTO> GetUserByUserID(Guid? userId)
    {
        ApplicationUser? user = await _usersRepository.GetUserByUserID(userId);
        return _mapper.Map<UserDTO>(user);
    }

    public async Task<AuthenticationResponse?> Login(LoginRequest loginRequest)
    {
        ApplicationUser? user = await _usersRepository.GetUserByEmailAndPassword(loginRequest.Email, loginRequest.Password);

        if (user == null)
        {
            return null;
        }

        // Generate new refresh token
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        // Update the user's refresh token in the DB
        user = user with
        {
            RefreshToken = newRefreshToken,
            RefreshTokenExpiryTime = refreshTokenExpiry
        };
        await _usersRepository.UpdateUser(user);

        // Generate JWT access token
        var accessToken = _jwtService.GenerateToken(_mapper.Map<AuthenticationResponse>(user));

        // Return mapped response with real tokens
        return _mapper.Map<AuthenticationResponse>(user) with
        {
            Success = true,
            Token = accessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task<AuthenticationResponse?> Register(RegisterRequest registerRequest)
    {
        // Create a new ApplicationUser object from RegisterRequest
        ApplicationUser user = _mapper.Map<ApplicationUser>(registerRequest) with
        {
            UserID = Guid.NewGuid(),
            RefreshToken = _jwtService.GenerateRefreshToken(),
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
        };

        ApplicationUser? registeredUser = await _usersRepository.AddUser(user);
        if (registeredUser == null)
        {
            return null;
        }

        // Generate access token
        var accessToken = _jwtService.GenerateToken(_mapper.Map<AuthenticationResponse>(registeredUser));

        // Return success response
        return _mapper.Map<AuthenticationResponse>(registeredUser) with
        {
            Success = true,
            Token = accessToken
        };
    }

    public async Task<AuthenticationResponse?> RefreshToken(RefreshTokenRequest refreshRequest)
    {
        if (string.IsNullOrEmpty(refreshRequest?.RefreshToken))
            return null;

        ApplicationUser? user = await _usersRepository.GetUserByRefreshToken(refreshRequest.RefreshToken);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null;

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateToken(_mapper.Map<AuthenticationResponse>(user));
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        
        // Create a *new* ApplicationUser record with updated refresh token values
        var updatedUser = user with
        {
            RefreshToken = newRefreshToken,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
        };

        // Save to DB
        await _usersRepository.UpdateUser(updatedUser);

        // Map updated user to AuthenticationResponse and return
        return _mapper.Map<AuthenticationResponse>(updatedUser) with
        {
            Success = true,
            Token = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}