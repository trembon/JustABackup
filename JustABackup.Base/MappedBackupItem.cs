using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Base
{
    public class MappedBackupItem
    {
        public BackupItem Output { get; set; }

        public IEnumerable<BackupItem> Input { get; set; }
    }
}
