using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models.Job
{
    public class JobDetailModel : BaseViewModel
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string CronSchedule { get; set; }

        public bool HasChangedModel { get; set; }

        public string BackupProvider { get; set; }

        public string StorageProvider { get; set; }

        public IEnumerable<string> TransformProviders { get; set; }

        public JobDetailModel()
        {
            TransformProviders = new List<string>();
        }
    }
}
