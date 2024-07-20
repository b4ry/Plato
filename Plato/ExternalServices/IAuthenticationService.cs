using Plato.Models.DTOs;

namespace Plato.ExternalServices
{
    public interface IAuthenticationService
    {
        public Task<string?> GetAuthenticationToken(UserLoginRequest loginRequest);
        public Task<string> RegisterUser(UserRegisterRequest registerRequest);
    }
}
