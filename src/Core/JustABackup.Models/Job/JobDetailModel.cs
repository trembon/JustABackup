using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models.Job
{
    public class JobDetailModel : BaseViewModel
    {
        public int ID { get; set; }

        public string Name { get; set; }

        [Display(Name = "Schedule")]
        public string CronSchedule { get; set; }

        public bool HasChangedModel { get; set; }

        [Display(Name = "Backup Provider")]
        public string BackupProvider { get; set; }

        [Display(Name = "Storage Provider")]
        public string StorageProvider { get; set; }

        [Display(Name = "Transform Providers")]
        public IEnumerable<string> TransformProviders { get; set; }

        public JobDetailModel()
        {
            TransformProviders = new List<string>();
        }
    }
}
