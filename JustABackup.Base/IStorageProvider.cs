using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Base
{
    public interface IStorageProvider
    {
        Task<bool> StoreItem(BackupItem item, Stream source);
    }
}
