using JustABackup.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace JustABackup.Plugin.ZipTransformer
{
    public class ZipTransformer : ITransformProvider
    {
        [Display(Name = "Single output file (<name>.zip)")]
        public string OutputFile { get; set; }

        [Display(Name = "If a single zip file should be created")]
        public bool CreateSingleFile { get; set; }

        public async Task TransformItem(BackupItem output, Stream outputStream, Dictionary<BackupItem, Stream> inputFiles)
        {
            using (ZipArchive zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
            {
                foreach (KeyValuePair<BackupItem, Stream> file in inputFiles)
                {
                    ZipArchiveEntry entry = zipArchive.CreateEntry(file.Key.FullPath);
                    using (var entryStream = entry.Open())
                    {
                        await file.Value.CopyToAsync(entryStream);
                    }
                }
            }
        }

        public Task<MappedBackupItemList> MapInput(IEnumerable<BackupItem> input)
        {
            MappedBackupItemList result = new MappedBackupItemList();

            if (CreateSingleFile)
            {
                BackupItem zippedBackupItem = new BackupItem();
                zippedBackupItem.Name = $"{OutputFile}.zip";
                zippedBackupItem.Path = "";

                result.Add(zippedBackupItem, input);
            }
            else
            {
                foreach (BackupItem file in input)
                {
                    BackupItem zippedBackupItem = new BackupItem();
                    zippedBackupItem.Name = $"{file.Name}.<ip";
                    zippedBackupItem.Path = file.Path;

                    result.Add(zippedBackupItem, file);
                }
            }

            return Task.FromResult(result);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
