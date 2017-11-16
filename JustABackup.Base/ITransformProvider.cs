using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Base
{
    public interface ITransformProvider
    {
        Task<Dictionary<BackupItem, IEnumerable<BackupItem>>> TransformList(IEnumerable<BackupItem> files);

        Task<Stream> TransformItem(Dictionary<BackupItem, Stream> backupItems);
    }
}
