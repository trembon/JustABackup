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
        private IScheduler scheduler;

        public SchedulerService(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public async Task CreateScheduledJob(int jobId, string cronSchedule)
        {
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

        public async Task<DateTime?> GetNextRunTime(int jobId)
        {
            JobKey jobKey = JobKey.Create(jobId.ToString());
            var triggers = await scheduler.GetTriggersOfJob(jobKey);

            return triggers.FirstOrDefault()?.GetNextFireTimeUtc().Value.UtcDateTime;
        }

        public async Task PauseJob(int jobId)
        {
            JobKey jobKey = JobKey.Create(jobId.ToString());
            await scheduler.PauseJob(jobKey);
        }

        public async Task ResumeJob(int jobId)
        {
            JobKey jobKey = JobKey.Create(jobId.ToString());
            await scheduler.ResumeJob(jobKey);
        }

        public async Task TriggerJob(int jobId)
        {
            JobKey jobKey = JobKey.Create(jobId.ToString());
            if (await scheduler.CheckExists(jobKey))
                await scheduler.TriggerJob(jobKey);
        }
    }
}
