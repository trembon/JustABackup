using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Base
{
    /// <summary>
    /// A provider that will supply files to be backuped.
    /// Files read by this provider will later be stored by a storage provider.
    /// Note: The purpose for this provider is to be read-only.
    /// </summary>
    public interface IBackupProvider : IDisposable
    {
        /// <summary>
        /// Gets a list of files to backup.
        /// Example: return data from File.GetFiles(folder);
        /// </summary>
        /// <param name="lastRun">Timestamp when the last successfull backup started.</param>
        /// <returns>A list of files to backup.</returns>
        Task<IEnumerable<BackupItem>> GetItems(DateTime? lastRun);

        /// <summary>
        /// Opens a readable stream to a file, returned by the GetItems method.
        /// Note: The stream should not be closed or disposed before beging returned.
        /// Example: return the stream from File.OpenRead(path);
        /// </summary>
        /// <param name="item">The file to open a stream to, returned by the GetItems method.</param>
        /// <returns>An open stream to a file.</returns>
        Task<Stream> OpenRead(BackupItem item);
    }
}
