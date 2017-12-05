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

        public async Task<string> GetCronSchedule(int jobId)
        {
            JobKey jobKey = JobKey.Create(jobId.ToString());
            var triggers = await scheduler.GetTriggersOfJob(jobKey);

            var defaultTrigger = triggers.FirstOrDefault() as Quartz.Impl.Triggers.CronTriggerImpl;
            if (defaultTrigger != null)
                return defaultTrigger.CronExpressionString;

            return null;
        }

        public async Task<DateTime?> GetNextRunTime(int jobId)
        {
            JobKey jobKey = JobKey.Create(jobId.ToString());
            var triggers = await scheduler.GetTriggersOfJob(jobKey);
            if (triggers.Count != 1)
                return null;

            var trigger = triggers.FirstOrDefault();
            var triggerState = await scheduler.GetTriggerState(trigger.Key);

            if (triggerState == TriggerState.Paused)
                return null;

            return trigger.GetNextFireTimeUtc()?.LocalDateTime;
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
