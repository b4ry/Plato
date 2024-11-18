using Plato.Constants;
using Plato.Models.DTOs;

namespace Plato.ExternalServices
{
    public interface IAuthenticationService
    {
        public Task<string?> GetAuthenticationToken(UserLoginRequest loginRequest);
        public Task<RegisterStatuses> RegisterUser(UserRegisterRequest registerRequest);
    }
}
