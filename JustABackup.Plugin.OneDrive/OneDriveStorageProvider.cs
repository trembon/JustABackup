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
        public string Folder { get; set; }

        public IAuthenticatedClient<IOneDriveClient> Client { get; set; }

        public async Task<bool> StoreItem(BackupItem item, Stream source)
        {
            string itemPath = Path.Combine(Folder, item.FullPath);

            var client = await Client.GetClient();
            await client.Drive.Root.ItemWithPath(itemPath).Content.Request().PutAsync<Item>(source);
            return true;
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}
