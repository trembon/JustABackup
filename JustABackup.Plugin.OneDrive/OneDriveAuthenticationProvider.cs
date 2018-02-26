using JustABackup.Base;
using Microsoft.OneDrive.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace JustABackup.Plugin.OneDrive
{
    [DisplayName("OneDrive Authentication")]
    public class OneDriveAuthenticationProvider : IAuthenticationProvider<IOneDriveClient>
    {
        [Display(Name = "Client ID")]
        public string ClientID { get; set; }

        [PasswordPropertyText]
        [Display(Name = "Client Secret")]
        public string ClientSecret { get; set; }

        private string callbackUrl;

        private Action<string> storeSession;

        public void Initialize(string callbackUrl, Action<string> storeSession)
        {
            this.callbackUrl = callbackUrl;
            this.storeSession = storeSession;
        }

        public async Task<bool> Authenticate(Dictionary<string, string> data)
        {
            OAuthSession session = await AuthenticationHelper.RequestRefreshToken(ClientID, ClientSecret, callbackUrl, data["code"]);
            
            if (session == null || !session.IsValid())
                throw new ArgumentNullException("Failed to retreive refresh_token");

            storeSession?.Invoke(JsonConvert.SerializeObject(session));

            return true;
        }

        public IOneDriveClient GetAuthenticatedClient(string storedSession)
        {
            OAuthSession session = JsonConvert.DeserializeObject<OAuthSession>(storedSession);
            if (string.IsNullOrWhiteSpace(session.RefreshToken))
                throw new ArgumentNullException(nameof(session.RefreshToken));

            RefreshTokenAuthenticationProvider authenticationProvider = new RefreshTokenAuthenticationProvider(ClientID, ClientSecret, callbackUrl, session, this.storeSession);
            IOneDriveClient client = new OneDriveClient("https://api.onedrive.com/v1.0", authenticationProvider);

            return client;
        }

        public Task<string> GetOAuthUrl()
        {
            string encodingCallbackUrl = HttpUtility.UrlEncode(callbackUrl);

            return Task.FromResult($"https://login.live.com/oauth20_authorize.srf?client_id={ClientID}&scope=onedrive.readwrite wl.signin wl.offline_access&response_type=code&redirect_uri={encodingCallbackUrl}");
        }
    }
}
