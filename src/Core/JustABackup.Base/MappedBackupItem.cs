using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Base
{
    /// <summary>
    /// Class to represent an output transform of one or multiple files.
    /// </summary>
    public class MappedBackupItem
    {
        /// <summary>
        /// The file that will be the result of all the files in the Input property.
        /// </summary>
        public BackupItem Output { get; set; }

        /// <summary>
        /// List of files that should be transformed to the Output property.
        /// </summary>
        public IEnumerable<BackupItem> Input { get; set; }
    }
}
