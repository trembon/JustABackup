using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Base
{
    public sealed class MappedBackupItemList : List<MappedBackupItem>
    {
        public void Add(BackupItem output, IEnumerable<BackupItem> input)
        {
            this.Add(new MappedBackupItem { Output = output, Input = input });
        }

        public void Add(BackupItem output, BackupItem input)
        {
            this.Add(output, new BackupItem[] { input });
        }
    }
}
