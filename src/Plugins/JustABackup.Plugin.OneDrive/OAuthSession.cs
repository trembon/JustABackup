using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Plugin.OneDrive
{
    public class OAuthSession
    {
        public string RefreshToken { get; set; }

        public string AccessToken { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public OAuthSession()
        {
        }

        public OAuthSession(string refreshToken)
        {
            this.RefreshToken = refreshToken;
        }

        public OAuthSession(string refreshToken, string accessToken, int secondsToExpire)
            : this(refreshToken)
        {
            this.AccessToken = accessToken;
            this.ExpiresAt = DateTime.Now.AddSeconds(secondsToExpire);
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(RefreshToken))
                return false;

            if (string.IsNullOrWhiteSpace(AccessToken))
                return false;

            if (ExpiresAt == null || ExpiresAt < DateTime.Now.AddMinutes(5))
                return false;

            return true;
        }
    }
}
