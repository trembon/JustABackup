using JustABackup.Base;
using JustABackup.Core.Services;
using JustABackup.Database;
using JustABackup.Database.Entities;
using JustABackup.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.Entities
{
    [Serializable]
    public class AuthenticatedClient<T> : IAuthenticatedClient<T> where T : class
    {
        [NonSerialized]
        private T client;

        [NonSerialized]
        private Func<int, Task<object>> getClient;

        public int ID { get; }

        public AuthenticatedClient(int id)
        {
            this.ID = id;
        }

        public async Task<T> GetClient()
        {
            if (client == null)
                client = await getClient?.Invoke(ID) as T;

            return client;
        }

        internal void SetLoadMethod(Func<int, Task<object>> getClient)
        {
            this.getClient = getClient;
        }

        public override string ToString()
        {
            return ID.ToString();
        }

        public void Dispose()
        {
            if(client != null && client is IDisposable)
                ((IDisposable)client).Dispose();
        }
    }
}
