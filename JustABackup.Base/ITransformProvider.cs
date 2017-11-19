using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Base
{
    public interface ITransformProvider
    {
        Task<IEnumerable<MappedBackupItem>> TransformList(IEnumerable<BackupItem> files);

        Task TransformItem(Stream transformStream, Dictionary<BackupItem, Stream> inputFiles);
    }
}
