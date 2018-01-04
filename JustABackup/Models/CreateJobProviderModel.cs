using JustABackup.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models
{
    public class CreateJobProviderModel : CreateProviderModel
    {
        public virtual int ID { get; set; }

        public int CurrentIndex { get; set; }
    }
}
