using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Plugin.OneDrive
{
    public class RefreshTokenAuthenticationProvider : IAuthenticationProvider
    {
        public string ClientID { get; }

        public string ClientSecret { get; }

        public string RedirectUri { get; }
        
        public OAuthSession Session { get; }
        
        public RefreshTokenAuthenticationProvider(string clientId, string clientSecret, string redirectUri, string refreshToken)
            : this(clientId, clientSecret, redirectUri, new OAuthSession(refreshToken))
        {
        }

        public RefreshTokenAuthenticationProvider(string clientId, string clientSecret, string redirectUri, OAuthSession session)
        {
            this.Session = session;
            this.ClientID = clientId;
            this.ClientSecret = ClientSecret;
            this.RedirectUri = redirectUri;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            if (!Session.IsValid())
                await AuthenticationHelper.RequestAccessToken(ClientID, ClientSecret, RedirectUri, Session.RefreshToken);

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.AccessToken);
        }
    }
}
