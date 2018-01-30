using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models.Job
{
    public class CreateJobProviderModel : CreateProviderModel
    {
        public virtual int ID { get; set; }

        public int CurrentIndex { get; set; }
    }
}
