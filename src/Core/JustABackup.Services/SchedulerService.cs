using JustABackup.Core.ScheduledJobs;
using Quartz;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Services
{
    public interface ISchedulerService
    {
        Task CreateOrUpdate(int jobId, string cronSchedule);

        Task<DateTime?> GetNextRunTime(int jobId);

        Task TriggerJob(int jobId);

        Task PauseJob(int jobId);

        Task ResumeJob(int jobId);

        Task<string> GetCronSchedule(int jobId);
    }

    public class SchedulerService : ISchedulerService
    {
        private readonly IScheduler scheduler;

        public SchedulerService(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public async Task CreateOrUpdate(int jobId, string cronSchedule)
        {
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(jobId.ToString())
                .WithCronSchedule(cronSchedule)
                .ForJob(jobId.ToString())
                .Build();

            JobKey jobKey = JobKey.Create(jobId.ToString());
            IEnumerable<ITrigger> triggers = await scheduler.GetTriggersOfJob(jobKey);
            if(triggers != null && triggers.Count() == 1)
            {
                var triggerState = await scheduler.GetTriggerState(triggers.First().Key);
                await scheduler.RescheduleJob(triggers.First().Key, trigger);

                if (triggerState == TriggerState.Paused)
                    await PauseJob(jobId);
            }
            else
            {
                // TODO: fix
                //IJobDetail jobDetail = JobBuilder.Create<DefaultScheduledJob>()
                //        .WithIdentity(jobId.ToString())
                //        .Build();

                //await scheduler.ScheduleJob(jobDetail, trigger);
            }
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
