using JustABackup.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace JustABackup.Plugin.ZipTransformer
{
    public class ZipTransformer : ITransformProvider
    {
        public Task<Stream> TransformItem(Dictionary<BackupItem, Stream> backupItems)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<BackupItem, IEnumerable<BackupItem>>> TransformList(IEnumerable<BackupItem> files)
        {
            Dictionary<BackupItem, IEnumerable<BackupItem>> result = new Dictionary<BackupItem, IEnumerable<BackupItem>>();

            BackupItem zippedBackupItem = new BackupItem();
            zippedBackupItem.Name = "complete.zip";
            zippedBackupItem.Path = "";

            result.Add(zippedBackupItem, files);

            return Task.FromResult(result);
        }
    }
}
