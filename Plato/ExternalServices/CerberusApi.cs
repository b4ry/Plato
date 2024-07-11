using Newtonsoft.Json;
using Plato.DTOs;
using System.Configuration;
using System.Net.Http;
using System.Text;

namespace Plato.ExternalServices
{
    internal static class CerberusApi
    {
        internal static async Task<string?> GetAuthenticationToken(UserLoginRequest loginRequest)
        {
            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
            var endpoint = ConfigurationManager.AppSettings.Get("CerberusApiUrl") + "/Login";

            var httpResponse = await new HttpClient().PostAsync(endpoint, content);
            var httpResponseContent = await httpResponse.Content.ReadAsStringAsync();

            if (httpResponse.IsSuccessStatusCode)
            {
                return httpResponseContent;
            }

            return null;
        }

        internal static async Task<string> RegisterUser(UserRegisterRequest registerRequest)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(registerRequest), Encoding.UTF8, "application/json");
                var endpoint = ConfigurationManager.AppSettings.Get("CerberusApiUrl") + "/Register";

                var httpResponse = await new HttpClient().PostAsync(endpoint, content);
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
