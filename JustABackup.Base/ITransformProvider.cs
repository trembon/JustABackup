using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Base
{
    public interface ITransformProvider : IDisposable
    {
        Task<MappedBackupItemList> MapInput(IEnumerable<BackupItem> input);

        Task TransformItem(BackupItem output, Stream outputStream, Dictionary<BackupItem, Stream> inputFiles);
    }
}
