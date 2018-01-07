using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Base
{
    public interface IAuthenticatedClient<out T>
    {
        long ID { get; }

        T GetClient();
    }
}
