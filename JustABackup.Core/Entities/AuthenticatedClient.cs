using JustABackup.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Core.Entities
{
    public class AuthenticatedClient<T> : IAuthenticatedClient<T>
    {
        public long ID { get; }

        public AuthenticatedClient(long id)
        {
            this.ID = id;
        }

        public T GetClient()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return ID.ToString();
        }
    }
}
