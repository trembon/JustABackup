using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Base
{
    public interface IAuthenticatedClient<T> where T : class
    {
        long ID { get; }

        Task<T> GetClient();
    }
}
