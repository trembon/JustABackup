using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Base
{
    /// <summary>
    /// Class to represent a list of MappedBackupItems.
    /// </summary>
    public sealed class MappedBackupItemList : List<MappedBackupItem>
    {
        /// <summary>
        /// Creates and adds a MappedBackupItem to the list.
        /// </summary>
        /// <param name="output">The output file.</param>
        /// <param name="input">The input files.</param>
        public void Add(BackupItem output, IEnumerable<BackupItem> input)
        {
            this.Add(new MappedBackupItem { Output = output, Input = input });
        }

        /// <summary>
        /// Creates and adds a MappedBackupItem to the list.
        /// </summary>
        /// <param name="output">The output file.</param>
        /// <param name="input">The input file.</param>
        public void Add(BackupItem output, BackupItem input)
        {
            this.Add(output, new BackupItem[] { input });
        }
    }
}
