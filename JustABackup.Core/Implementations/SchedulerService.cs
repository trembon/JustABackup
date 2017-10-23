using JustABackup.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Quartz;
using System.Threading.Tasks;
using Quartz.Impl;
using System.Collections.Specialized;
using System.Linq;
using JustABackup.Core.ScheduledJobs;

namespace JustABackup.Core.Implementations
{
    public class SchedulerService : ISchedulerService
    {
        private static IScheduler scheduler = null;

        public async Task CreateScheduledJob(int jobId, string cronSchedule)
        {
            if (scheduler == null)
                throw new InvalidOperationException("Scheduler is not initilized");

            IJobDetail jobDetail = JobBuilder.Create<DefaultScheduledJob>()
                    .WithIdentity(jobId.ToString())
                    .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(jobId.ToString())
                .WithCronSchedule(cronSchedule)
                .ForJob(jobId.ToString())
                .Build();

            await scheduler.ScheduleJob(jobDetail, trigger);
        }

        public async Task CreateScheduler(NameValueCollection properties, IServiceProvider serviceProvider)
        {
            ISchedulerFactory sf = new StdSchedulerFactory(properties);

            scheduler = await sf.GetScheduler();
            scheduler.JobFactory = new JobFactory(serviceProvider);

            await scheduler.Start();

            // TODO: IJobFactory with DI
        }

        public async Task<DateTime?> GetNextRunTime(int jobId)
        {
            if (scheduler == null)
                throw new InvalidOperationException("Scheduler is not initilized");

            JobKey jobKey = JobKey.Create(jobId.ToString());
            var triggers = await scheduler.GetTriggersOfJob(jobKey);

            return triggers.FirstOrDefault()?.GetNextFireTimeUtc().Value.UtcDateTime;
        }

        public async Task TriggerJob(int jobId)
        {
            if (scheduler == null)
                throw new InvalidOperationException("Scheduler is not initilized");

            JobKey jobKey = JobKey.Create(jobId.ToString());
            if (await scheduler.CheckExists(jobKey))
                await scheduler.TriggerJob(jobKey);
        }
    }
}
