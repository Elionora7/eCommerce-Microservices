using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using eCommerce.Core.Entities;
using eCommerce.Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using eCommerce.Core.DTO;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Core.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
      

        public string GenerateToken(AuthenticationResponse user)
        {

            // Read JWT values - FALLBACK OPTIONS
            var jwtKey = _configuration["Jwt__Key"] ??   // From Docker environment
              _configuration["JWT_KEY"] ??            // Direct env variable
              _configuration["Jwt:Key"] ??            // From appsettings
              Environment.GetEnvironmentVariable("JWT_KEY") ??
              throw new ArgumentNullException("JWT key not configured");

            var issuer = _configuration["Jwt__Issuer"] ??
                         _configuration["JWT_ISSUER"] ??
                         _configuration["Jwt:Issuer"] ??
                         Environment.GetEnvironmentVariable("JWT_ISSUER") ??
                         throw new ArgumentNullException("JWT_ISSUER not set");

            var audience = _configuration["Jwt__Audience"] ??
                           _configuration["JWT_AUDIENCE"] ??
                           _configuration["Jwt:Audience"] ??
                           Environment.GetEnvironmentVariable("JWT_AUDIENCE") ??
                           throw new ArgumentNullException("JWT_AUDIENCE not set");

            // Validate key length
            if (Encoding.UTF8.GetByteCount(jwtKey) < 16)
            {
                throw new ArgumentException(
                    $"JWT key too short. Got {Encoding.UTF8.GetByteCount(jwtKey)} bytes " +
                    $"({Encoding.UTF8.GetByteCount(jwtKey) * 8} bits), need 16+ bytes (128+ bits)");
            }

            // Create claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim("name", user.Name ?? string.Empty)
            };

            // 6. Generate token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64]; // 512 bits
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
        public bool ValidateRefreshToken(string storedToken, string submittedToken)
        {
            // Basic validation
            if (string.IsNullOrEmpty(storedToken)) return false;
            if (string.IsNullOrEmpty(submittedToken)) return false;

            try
            {
                // Verify it's valid Base64 and correct length
                var decodedBytes = Convert.FromBase64String(submittedToken);
                if (decodedBytes.Length != 64) return false;
            }
            catch (FormatException)
            {
                return false; // Invalid Base64
            }

            // Time-constant comparison
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(storedToken),
                Encoding.UTF8.GetBytes(submittedToken)
            );
        }

    }
}

