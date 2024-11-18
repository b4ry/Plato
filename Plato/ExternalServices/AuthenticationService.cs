using Newtonsoft.Json;
using Plato.Constants;
using Plato.Models.DTOs;
using System.Net.Http;
using System.Text;

namespace Plato.ExternalServices
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private const string ApplicationJsonContentType = "application/json";


        public AuthenticationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> GetAuthenticationToken(UserLoginRequest loginRequest)
        {
            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, ApplicationJsonContentType);

            var httpResponse = await _httpClient.PostAsync("login", content);
            var httpResponseContent = JsonConvert.DeserializeObject<AuthToken>(await httpResponse.Content.ReadAsStringAsync());

            if (httpResponse.IsSuccessStatusCode && httpResponseContent != null)
            {
                return httpResponseContent.AccessToken;
            }

            return null;
        }

        public async Task<RegisterStatuses> RegisterUser(UserRegisterRequest registerRequest)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(registerRequest), Encoding.UTF8, ApplicationJsonContentType);

                var httpResponse = await _httpClient.PostAsync("register", content);
                var httpResponseContent = JsonConvert.DeserializeObject<AuthToken>(await httpResponse.Content.ReadAsStringAsync());

                if (httpResponse.IsSuccessStatusCode && httpResponseContent != null)
                {
                    return RegisterStatuses.Registered;
                }

                return RegisterStatuses.Unregistered;
            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}
