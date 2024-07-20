using Newtonsoft.Json;
using Plato.Models.DTOs;
using System.Net.Http;
using System.Text;

namespace Plato.ExternalServices
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;

        public AuthenticationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> GetAuthenticationToken(UserLoginRequest loginRequest)
        {
            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

            var httpResponse = await _httpClient.PostAsync("login", content);
            var httpResponseContent = await httpResponse.Content.ReadAsStringAsync();

            if (httpResponse.IsSuccessStatusCode)
            {
                return httpResponseContent;
            }

            return null;
        }

        public async Task<string> RegisterUser(UserRegisterRequest registerRequest)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(registerRequest), Encoding.UTF8, "application/json");

                var httpResponse = await _httpClient.PostAsync("register", content);
                var httpResponseContent = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    return "Registered";
                }

                return "Not registered";
            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}
