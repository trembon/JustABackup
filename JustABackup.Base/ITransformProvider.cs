using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Base
{
    /// <summary>
    /// A provider that will transform files before being written to a storage provider.
    /// Example: Write multiple files from a backup provider to a zip file before being sent to a storage provider.
    /// </summary>
    public interface ITransformProvider : IDisposable
    {
        /// <summary>
        /// Map files from previous step to how the output file should look like after this transform provider.
        /// Example 1: Multiple files turn to a single .zip file.
        /// Example 2: Single file to multiple output files, one original file and one crc file.
        /// </summary>
        /// <param name="input">List if files from the previous step.</param>
        /// <returns>A mapped list of files.</returns>
        Task<MappedBackupItemList> MapInput(IEnumerable<BackupItem> input);

        /// <summary>
        /// Transforms a single or list of files into a single output.
        /// This method will be called based on the output from the MapInput method.
        /// Output file should be written to the outputStream parameter.
        /// Example 1: Multiple files will be added to a .zip file.
        /// Example 2: Single file will be encrypted,
        /// </summary>
        /// <param name="output">The file that will be outputed.</param>
        /// <param name="outputStream">Stream that the output file will be written to.</param>
        /// <param name="inputFiles">File(s) to transform.</param>
        /// <returns>An awaitable task.</returns>
        Task TransformItem(BackupItem output, Stream outputStream, Dictionary<BackupItem, Stream> inputFiles);
    }
}
