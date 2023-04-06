using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Plugin.OneDrive
{
    public class OneDriveClient
    {
        private Action<string> storeSession;

        public string ClientID { get; }

        public string ClientSecret { get; }

        public string RedirectUri { get; }
        
        public OAuthSession Session { get; private set; }
        
        public OneDriveClient(string clientId, string clientSecret, string redirectUri, string refreshToken)
            : this(clientId, clientSecret, redirectUri, new OAuthSession(refreshToken), null)
        {
        }

        public OneDriveClient(string clientId, string clientSecret, string redirectUri, OAuthSession session, Action<string> storeSession)
        {
            this.Session = session;
            this.ClientID = clientId;
            this.ClientSecret = clientSecret;
            this.RedirectUri = redirectUri;
            this.storeSession = storeSession;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            if (!Session.IsValid())
            {
                Session = await AuthenticationHelper.RequestAccessToken(ClientID, ClientSecret, RedirectUri, Session.RefreshToken);
                storeSession?.Invoke(JsonConvert.SerializeObject(Session));
            }

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.AccessToken);
        }
    }
}
