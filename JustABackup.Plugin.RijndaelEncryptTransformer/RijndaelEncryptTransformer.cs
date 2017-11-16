using JustABackup.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace JustABackup.Plugin.RijndaelEncryptTransformer
{
    public class RijndaelEncryptTransformer : ITransformProvider
    {
        public Task<Stream> TransformItem(Dictionary<BackupItem, Stream> backupItems)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<BackupItem, IEnumerable<BackupItem>>> TransformList(IEnumerable<BackupItem> files)
        {
            Dictionary<BackupItem, IEnumerable<BackupItem>> result = new Dictionary<BackupItem, IEnumerable<BackupItem>>();

            foreach(var file in files)
            {
                BackupItem encryptedBackupItem = new BackupItem();
                encryptedBackupItem.Name = $"{file.Name}.enc";
                encryptedBackupItem.Path = file.Path;

                result.Add(encryptedBackupItem, new []{ file });
            }

            return Task.FromResult(result);
        }
    }
}
