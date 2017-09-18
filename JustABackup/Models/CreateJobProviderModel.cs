using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models
{
    public class CreateJobProviderModel
    {
        public string Action { get; set; }

        public string ProviderName { get; set; }
        
        public List<ProviderProperty> Properties { get; set; }
    }
}
