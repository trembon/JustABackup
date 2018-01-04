using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Base
{
    public interface IAuthenticationProvider<out T> where T : class
    {
        void Initialize(string callbackUrl, Action<string> storeSession);

        Task<string> GetOAuthUrl();

        Task<bool> Authenticate(Dictionary<string, string> data);

        T GetAuthenticatedClient(string storedSession);
    }
}
