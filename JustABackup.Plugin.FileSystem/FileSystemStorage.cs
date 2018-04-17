using JustABackup.Base;
using JustABackup.Base.Attributes;
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
        [Transform]
        [Display(Name = "Folder")]
        public string TargetFolder { get; set; }

        public async Task<bool> StoreItem(BackupItem item, Stream source)
        {
            string path = Path.Combine(TargetFolder, item.FullPath);
            if (item.FullPath.StartsWith("/"))
                path = Path.Combine(TargetFolder, item.FullPath.Substring(1));

            string folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            using (Stream target = File.OpenWrite(path))
            {
                await source.CopyToAsync(target);
            }
            return true;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
