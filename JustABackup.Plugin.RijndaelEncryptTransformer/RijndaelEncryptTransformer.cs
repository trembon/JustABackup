using JustABackup.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace JustABackup.Plugin.RijndaelEncryptTransformer
{
    public class RijndaelEncryptTransformer : ITransformProvider
    {
        public Task TransformItem(Stream transformStream, Dictionary<BackupItem, Stream> inputFiles)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MappedBackupItem>> TransformList(IEnumerable<BackupItem> files)
        {
            List<MappedBackupItem> result = new List<MappedBackupItem>();

            foreach(var file in files)
            {
                BackupItem encryptedBackupItem = new BackupItem();
                encryptedBackupItem.Name = $"{file.Name}.enc";
                encryptedBackupItem.Path = file.Path;

                result.Add(new MappedBackupItem { Output = encryptedBackupItem, Input = new[] { file } });
            }

            return Task.FromResult((IEnumerable<MappedBackupItem>)result);
        }
    }
}
