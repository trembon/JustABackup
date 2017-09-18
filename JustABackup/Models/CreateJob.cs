using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models
{
    public class CreateJob
    {
        public CreateJobModel Base { get; set; }

        public CreateJobProviderModel BackupProvider { get; set; }

        public CreateJobProviderModel StorageProvider { get; set; }
    }
}
