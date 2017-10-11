using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.Core.Entities.Database
{
    internal class BackupJob
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }
        
        [Required]
        public ProviderInstance StorageProvider { get; set; }

        [Required]
        public ProviderInstance BackupProvider { get; set; }

        public List<BackupJobHistory> History { get; set; }

        public BackupJob()
        {
            History = new List<BackupJobHistory>();
        }
    }
}
