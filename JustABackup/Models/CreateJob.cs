using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models
{
    public class CreateJob
    {
        public int[] ProviderIDs
        {
            get
            {
                if (Base != null)
                {
                    List<int> ids = new List<int>();
                    ids.Add(Base.BackupProvider);
                    ids.AddRange(Base.TransformProvider);
                    ids.Add(Base.StorageProvider);
                    return ids.ToArray();
                }

                return new int[0];
            }
        }

        public CreateJobModel Base { get; set; }

        //public ConfigureJobProviderModel BackupProvider { get; set; }

        //public ConfigureJobProviderModel StorageProvider { get; set; }

        public List<ConfigureJobProviderModel> Providers { get; set; }

        public CreateJob()
        {
            Providers = new List<ConfigureJobProviderModel>();
        }
    }
}
