using JustABackup.Base;
using Microsoft.OneDrive.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Plugin.OneDrive
{
    public class OneDriveStorageProvider : IStorageProvider
    {
        public IAuthenticatedClient<IOneDriveClient> Client { get; set; }

        public Task<bool> StoreItem(BackupItem item, Stream source)
        {
            throw new NotImplementedException();
        }
    }
}
