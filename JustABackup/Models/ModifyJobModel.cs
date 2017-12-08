using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models
{
    public class ModifyJobModel : CreateJobModel
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int ID { get; set; }
    }
}
