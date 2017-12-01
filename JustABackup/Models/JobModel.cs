using System;

namespace JustABackup.Models
{
    public class JobModel
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string BackupProvider { get; set; }

        public string StorageProvider { get; set; }

        public bool HasChangedModel { get; set; }

        public DateTime? NextRun { get; set; }
    }
}