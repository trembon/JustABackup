using JustABackup.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace JustABackup.Plugin.FileSystem
{
    [DisplayName("File System Storage")]
    public class FileSystemStorage : IStorageProvider
    {
        [Display(Name = "Folder")]
        public string TargetFolder { get; set; }

        public async Task<bool> StoreItem(BackupItem item, Stream source)
        {
            try
            {
                using (Stream target = File.OpenWrite(Path.Combine(TargetFolder, item.Path, item.Name)))
                {
                    await source.CopyToAsync(target);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
