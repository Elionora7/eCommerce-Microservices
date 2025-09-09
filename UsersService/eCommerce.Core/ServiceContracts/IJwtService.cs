using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;

namespace eCommerce.Core.ServiceContracts
{
    public interface IJwtService
    {
        string GenerateToken(AuthenticationResponse user);
        string GenerateRefreshToken();
        bool ValidateRefreshToken(string storedToken, string submittedToken);

    }
}
