using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models.Home
{
    public class ListJobHistoryModel : BaseViewModel
    {
        public List<JobHistoryModel> JobHistory { get; set; }
    }
}
