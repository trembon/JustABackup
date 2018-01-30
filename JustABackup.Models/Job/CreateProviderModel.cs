using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models.Job
{
    public class CreateProviderModel : BaseViewModel
    {
        public string ProviderName { get; set; }

        public List<ProviderPropertyModel> Properties { get; set; }
    }
}
