using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models.Job
{
    public class ListJobsModel : BaseViewModel
    {
        public List<JobModel> Jobs { get; set; }
    }
}
