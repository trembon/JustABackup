using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Base
{
    /// <summary>
    /// A provider that will write files to a storage.
    /// Files stored by this provider can come directly from a backup provider or have been modified by a transform provider.
    /// Note: The purpose for this provider is to be write-only.
    /// </summary>
    public interface IStorageProvider : IDisposable
    {
        /// <summary>
        /// Writes a file to the storage, from the open stream.
        /// Note: The stream can be modified by a transform provider before being delivered from a backup provider.
        /// </summary>
        /// <param name="item">The metadata of the file to be written.</param>
        /// <param name="source">An open stream to the file to write.</param>
        /// <returns>If the file was written successfully.</returns>
        Task<bool> StoreItem(BackupItem item, Stream source);
    }
}
