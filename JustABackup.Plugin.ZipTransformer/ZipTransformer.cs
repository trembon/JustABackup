using JustABackup.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace JustABackup.Plugin.ZipTransformer
{
    public class ZipTransformer : ITransformProvider
    {
        public async Task TransformItem(BackupItem item, Stream transformStream, Dictionary<BackupItem, Stream> inputFiles)
        {
            using (ZipArchive zipArchive = new ZipArchive(transformStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in inputFiles)
                {
                    ZipArchiveEntry entry = zipArchive.CreateEntry(file.Key.FullPath);
                    using (var entryStream = entry.Open())
                    {
                        await file.Value.CopyToAsync(entryStream);
                    }
                }
            }
        }

        public Task<IEnumerable<MappedBackupItem>> TransformList(IEnumerable<BackupItem> files)
        {
            List<MappedBackupItem> result = new List<MappedBackupItem>();

            BackupItem zippedBackupItem = new BackupItem();
            zippedBackupItem.Name = "complete.zip";
            zippedBackupItem.Path = "";

            result.Add(new MappedBackupItem { Output = zippedBackupItem, Input = files });

            return Task.FromResult((IEnumerable<MappedBackupItem>)result);
        }
    }
}
