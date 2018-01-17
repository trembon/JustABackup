using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Base
{
    public interface IBackupProvider
    {
        Task<IEnumerable<BackupItem>> GetItems(DateTime? lastRun);

        Task<Stream> OpenRead(BackupItem item);
    }
}
