using Newtonsoft.Json;
using Plato.DTOs;
using System.Configuration;
using System.Net.Http;
using System.Text;

namespace Plato.ExternalServices
{
    internal static class CerberusApi
    {
        internal static async Task<string?> GetAuthenticationToken(LoginRequest loginRequest)
        {
            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
            var httpResponse = await new HttpClient().PostAsync(ConfigurationManager.AppSettings.Get("CerberusApiUrl")!, content);

            var httpResponseContent = await httpResponse.Content.ReadAsStringAsync();

            if (httpResponse.IsSuccessStatusCode)
            {
                return httpResponseContent;
            }

            return null;
        }
    }
}
