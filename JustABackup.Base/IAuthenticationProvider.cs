using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

// TODO: idisposable

namespace JustABackup.Base
{
    /// <summary>
    /// A provider to handle API clients that require OAuth authentication.
    /// Clients will be loaded from IAuthenticatedClient on other providers.
    /// </summary>
    /// <typeparam name="T">Type of the API client that requires authentication.</typeparam>
    public interface IAuthenticationProvider<out T> where T : class
    {
        /// <summary>
        /// Initializes the provider with information supplies by the application.
        /// </summary>
        /// <param name="callbackUrl">The callback URL for this provider instance for OAuth2.</param>
        /// <param name="storeSession">The action to store session data in the database.</param>
        void Initialize(string callbackUrl, Action<string> storeSession);

        /// <summary>
        /// Gets the OAuth redirect URL for the authentication provider.
        /// If the callback URL is needed to the application, it should be used from the Initialize method.
        /// </summary>
        /// <returns>The URL to redirect to for OAuth authentication.</returns>
        Task<string> GetOAuthUrl();

        /// <summary>
        /// Authenticate the user with the data return from the query parameters from the OAuth login.
        /// </summary>
        /// <param name="data">The query parameters from the login.</param>
        /// <returns>If the authentication was successfull or not.</returns>
        Task<bool> Authenticate(Dictionary<string, string> data);

        /// <summary>
        /// Gets the authenticated API client from the provider.
        /// This method will be proxied from the IAuthenticatedClient.GetClient() method in other providers. 
        /// </summary>
        /// <param name="storedSession">The stored session data, stored with the storeSession parameter in the Initialize method.</param>
        /// <returns>The authenticated API client.</returns>
        T GetAuthenticatedClient(string storedSession);
    }
}
