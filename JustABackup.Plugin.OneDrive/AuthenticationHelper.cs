using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Plugin.OneDrive
{
    public static class AuthenticationHelper
    {
        public static async Task<OAuthSession> RequestRefreshToken(string clientId, string clientSecret, string redirectUri, string code)
        {
            return await ExecuteRequest(clientId, clientSecret, redirectUri, parameters =>
            {
                parameters.Add(new KeyValuePair<string, string>("code", code));
                parameters.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));  
            });
        }

        public static async Task<OAuthSession> RequestAccessToken(string clientId, string clientSecret, string redirectUri, string refreshToken)
        {
            return await ExecuteRequest(clientId, clientSecret, redirectUri, parameters =>
            {
                parameters.Add(new KeyValuePair<string, string>("refresh_token", refreshToken));
                parameters.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
            });
        }

        private static async Task<OAuthSession> ExecuteRequest(string clientId, string clientSecret, string redirectUri, Action<List<KeyValuePair<string, string>>> configure)
        {
            using (HttpClient client = new HttpClient())
            {
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                };

                configure?.Invoke(parameters);
                
                var result = await client.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token", new FormUrlEncodedContent(parameters));
                string resultString = await result.Content.ReadAsStringAsync();

                JContainer container = JsonConvert.DeserializeObject(resultString) as JContainer;

                string refreshToken = container["refresh_token"].Value<string>();
                string accessToken = container["access_token"].Value<string>();
                int expiresIn = container["expires_in"].Value<int>();

                return new OAuthSession(refreshToken, accessToken, expiresIn);
            }
        }
    }
}
