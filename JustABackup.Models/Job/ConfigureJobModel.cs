using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models.Job
{
    public class ConfigureJobModel : BaseViewModel
    {
        public int? ID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        public string CronSchedule { get; set; }

        [Required]
        public int StorageProvider { get; set; }

        [Required]
        public int BackupProvider { get; set; }

        public int[] TransformProviders { get; set; }
        
        public IEnumerable<Dictionary<string, string>> Providers { get; set; }

        public Dictionary<int, int> ProviderInstances { get; set; }

        public ConfigureJobModel()
        {
            ProviderInstances = new Dictionary<int, int>();
            TransformProviders = new int[0];
        }

        public int[] GetProviderIDs()
        {
            List<int> ids = new List<int>();
            ids.Add(BackupProvider);

            if (TransformProviders != null)
                ids.AddRange(TransformProviders);

            ids.Add(StorageProvider);
            return ids.ToArray();
        }
    }
}
