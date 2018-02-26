using JustABackup.Base;
using Microsoft.OneDrive.Sdk;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Plugin.OneDrive
{
    [DisplayName("OneDrive Storage")]
    public class OneDriveStorageProvider : IStorageProvider
    {
        public string Folder { get; set; }

        public IAuthenticatedClient<IOneDriveClient> Client { get; set; }

        public async Task<bool> StoreItem(BackupItem item, Stream source)
        {
            string path = Path.Combine(Folder, item.FullPath);
            if (item.FullPath.StartsWith("/"))
                path = Path.Combine(Folder, item.FullPath.Substring(1));

            var client = await Client.GetClient();
            await client.Drive.Root.ItemWithPath(path).Content.Request().PutAsync<Item>(source);
            return true;
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}
