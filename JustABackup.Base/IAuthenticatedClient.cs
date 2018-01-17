using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Base
{
    public interface IAuthenticatedClient<T> : IDisposable where T : class
    {
        int ID { get; }

        Task<T> GetClient();
    }
}
