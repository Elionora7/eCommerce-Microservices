
using eCommerce.Core.DTO;
using eCommerce.Core.RepositoryContracts;
using eCommerce.Core.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers
{
    [Route("api/[controller]")] //api/auth
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly IJwtService _jwtService;

        public AuthController(IUsersService usersService, IJwtService jwtService)
        {
            _usersService = usersService;
            _jwtService = jwtService;
        }


        //Endpoint for user registration usecase
        //POST api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            //Check for invalid registerRequest
            if (registerRequest == null)
            {
                return BadRequest("Invalid registration data");
            }

            //Call the UsersService to handle registration
            AuthenticationResponse? authenticationResponse = await _usersService.Register(registerRequest);

            if (authenticationResponse == null || authenticationResponse.Success == false)
            {
                return BadRequest(authenticationResponse);
            }

            var token = _jwtService.GenerateToken(authenticationResponse);


            var responseWithToken = authenticationResponse with { Success = true, Token = token };

            return Ok(responseWithToken);

        }


        //Endpoint for user login usecase
        //POST /api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            //Check for invalid LoginRequest
            if (loginRequest == null)
            {
                return BadRequest("Invalid login data");
            }

            AuthenticationResponse? authenticationResponse = await _usersService.Login(loginRequest);

            if (authenticationResponse == null || authenticationResponse.Success == false)
            {
                return Unauthorized(authenticationResponse);
            }

            // Generate JWT token for the authenticated user
            var token = _jwtService.GenerateToken(authenticationResponse);

            var responseWithToken = authenticationResponse with { Success = true, Token = token };

            return Ok(responseWithToken);
        }

        //Endpoint for user refresh-token usecase
        //POST /api/auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _usersService.RefreshToken(request);

            if (result == null)
                return Unauthorized(new { message = "Invalid or expired refresh token" });

            return Ok(result);
        }




    }
}
