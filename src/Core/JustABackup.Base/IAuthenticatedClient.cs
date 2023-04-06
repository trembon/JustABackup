using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Base
{
    /// <summary>
    /// A property to supply API clients that requires authentication.
    /// This interface should be used as a property on other providers and should not be implemented.
    /// This client works together with the Authentication Provider.
    /// </summary>
    /// <typeparam name="T">Type of the API client that requires authentication.</typeparam>
    public interface IAuthenticatedClient<T> : IDisposable where T : class
    {
        /// <summary>
        /// The internal ID of the authenticated session stored in the database.
        /// </summary>
        int ID { get; }

        /// <summary>
        /// Gets the authenticated API client from the GetAuthenticatedClient method on the matching AuthenticationProvider.
        /// </summary>
        /// <returns>An authenticated API client based on the type T.</returns>
        Task<T> GetClient();
    }
}
