using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models.Job
{
    public class ModifyJobProviderModel : CreateJobProviderModel
    {
        [Required]
        [Range(0, int.MaxValue)]
        public override int ID { get => base.ID; set => base.ID = value; }
    }
}
