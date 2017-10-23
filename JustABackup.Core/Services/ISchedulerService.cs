using Quartz;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.Services
{
    public interface ISchedulerService
    {
        Task CreateScheduler(NameValueCollection properties, IServiceProvider serviceProvider);

        Task CreateScheduledJob(int jobId, string cronSchedule);

        Task<DateTime?> GetNextRunTime(int jobId);

        Task TriggerJob(int jobId);
    }
}
