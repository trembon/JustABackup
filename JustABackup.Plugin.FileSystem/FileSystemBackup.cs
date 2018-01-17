using JustABackup.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Plugin.FileSystem
{
    [DisplayName("File System Backup")]
    public class FileSystemBackup : IBackupProvider
    {
        [Display(Name = "Folder")]
        public string TargetFolder { get; set; }

        [Display(Name = "Filter", Description = "Filter for specific files, ex: *.jpg")]
        public string FileFilter { get; set; }

        [Display(Name = "Subdirectories", Description = "If subdirectories should be included")]
        public bool Subdirectories { get; set; }

        public Task<IEnumerable<BackupItem>> GetItems(DateTime? lastRun)
        {
            string[] foundFiles = new string[0];
            if(string.IsNullOrWhiteSpace(FileFilter))
                foundFiles = Directory.GetFiles(TargetFolder);
            else
                foundFiles = Directory.GetFiles(TargetFolder, FileFilter, Subdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            
            IEnumerable<BackupItem> result = foundFiles.Select(x => new BackupItem
            {
                Name = Path.GetFileName(x),
                Path = Path.GetDirectoryName(x).Substring(TargetFolder.Length)
            });

            return Task.FromResult(result);
        }

        public Task<Stream> OpenRead(BackupItem item)
        {
            return Task.Run(() => File.OpenRead(Path.Combine(TargetFolder, item.Path, item.Name)) as Stream);
        }
    }
}
