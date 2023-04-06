using JustABackup.Core.Services;
using JustABackup.Tests.Helpers;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JustABackup.Tests
{
    public class SchedulerServiceTests
    {
        private const string CRON_EVERY_FIVE_MINUTES = "0 0/5 * * * ?";
        private const string CRON_EVERY_TEN_MINUTES = "0 0/10 * * * ?";

        private SchedulerService SetupService(IScheduler quartz)
        {
            return new SchedulerService(quartz);
        }

        [Fact]
        public async void AddJob()
        {
            var quartz = await SchedulerHelper.CreateScheduler();
            var service = SetupService(quartz);

            await service.CreateOrUpdate(1, CRON_EVERY_FIVE_MINUTES);

            bool jobAdded = await quartz.CheckExists(new JobKey(1.ToString()));

            Assert.True(jobAdded);
        }

        [Fact]
        public async void UpdateJob()
        {
            var quartz = await SchedulerHelper.CreateScheduler();
            var service = SetupService(quartz);
            
            await service.CreateOrUpdate(1, CRON_EVERY_FIVE_MINUTES);

            string cronSchedule = await service.GetCronSchedule(1);
            Assert.Equal(CRON_EVERY_FIVE_MINUTES, cronSchedule);

            await service.CreateOrUpdate(1, CRON_EVERY_TEN_MINUTES);
            string cronScheduleUpdated = await service.GetCronSchedule(1);
            Assert.Equal(CRON_EVERY_TEN_MINUTES, cronScheduleUpdated);
        }
        
        [Fact]
        public async void UpdateJob_StillRunning()
        {
            var quartz = await SchedulerHelper.CreateScheduler();
            var service = SetupService(quartz);

            await service.CreateOrUpdate(1, CRON_EVERY_FIVE_MINUTES);

            DateTime? nextRun = await service.GetNextRunTime(1);
            Assert.NotNull(nextRun);
            Assert.True(nextRun > DateTime.Now);

            await service.CreateOrUpdate(1, CRON_EVERY_TEN_MINUTES);

            DateTime? nextRunUpdated = await service.GetNextRunTime(1);
            Assert.NotNull(nextRunUpdated);
            Assert.True(nextRunUpdated > DateTime.Now);
        }

        [Fact]
        public async void UpdateJob_StillPaused()
        {
            var quartz = await SchedulerHelper.CreateScheduler();
            var service = SetupService(quartz);

            await service.CreateOrUpdate(1, CRON_EVERY_FIVE_MINUTES);
            await service.PauseJob(1);

            DateTime? nextRun = await service.GetNextRunTime(1);
            Assert.Null(nextRun);

            await service.CreateOrUpdate(1, CRON_EVERY_TEN_MINUTES);

            DateTime? nextRunUpdated = await service.GetNextRunTime(1);
            Assert.Null(nextRunUpdated);
        }

        [Fact]
        public async void GetCronStringForJob()
        {
            var quartz = await SchedulerHelper.CreateScheduler();
            var service = SetupService(quartz);

            await service.CreateOrUpdate(1, CRON_EVERY_FIVE_MINUTES);

            string cronSchedule = await service.GetCronSchedule(1);
            Assert.Equal(CRON_EVERY_FIVE_MINUTES, cronSchedule);
        }

        [Fact]
        public async void GetNextRunTime_ExistingJob()
        {
            var quartz = await SchedulerHelper.CreateScheduler();
            var service = SetupService(quartz);

            await service.CreateOrUpdate(1, CRON_EVERY_FIVE_MINUTES);

            DateTime? nextRun = await service.GetNextRunTime(1);
            Assert.NotNull(nextRun);
            Assert.True(nextRun > DateTime.Now);
        }

        [Fact]
        public async void GetNextRunTime_NotExistingJob()
        {
            var quartz = await SchedulerHelper.CreateScheduler();
            var service = SetupService(quartz);
            
            DateTime? nextRun = await service.GetNextRunTime(2);
            Assert.Null(nextRun);
        }

        [Fact]
        public async void GetNextRunTime_PausedJob()
        {
            var quartz = await SchedulerHelper.CreateScheduler();
            var service = SetupService(quartz);

            await service.CreateOrUpdate(1, CRON_EVERY_FIVE_MINUTES);
            await service.PauseJob(1);

            DateTime? nextRun = await service.GetNextRunTime(1);
            Assert.Null(nextRun);
        }

        [Fact]
        public async void PauseJob_ExistingJob()
        {
            var quartz = await SchedulerHelper.CreateScheduler();
            var service = SetupService(quartz);

            await service.CreateOrUpdate(1, CRON_EVERY_FIVE_MINUTES);
            
            DateTime? nextRun = await service.GetNextRunTime(1);
            Assert.NotNull(nextRun);
            Assert.True(nextRun > DateTime.Now);

            await service.PauseJob(1);

            DateTime? nextRunPaused = await service.GetNextRunTime(1);
            Assert.Null(nextRunPaused);
        }
        
        [Fact]
        public async void PauseJob_NotExistingJob()
        {
            var quartz = await SchedulerHelper.CreateScheduler();
            var service = SetupService(quartz);
            
            await service.PauseJob(1);
        }

        [Fact]
        public async void ResumeJob_ExistingJob()
        {
            var quartz = await SchedulerHelper.CreateScheduler();
            var service = SetupService(quartz);

            await service.CreateOrUpdate(1, CRON_EVERY_FIVE_MINUTES);

            DateTime? nextRun = await service.GetNextRunTime(1);
            Assert.NotNull(nextRun);
            Assert.True(nextRun > DateTime.Now);

            await service.PauseJob(1);

            DateTime? nextRunPaused = await service.GetNextRunTime(1);
            Assert.Null(nextRunPaused);

            await service.ResumeJob(1);
            
            DateTime? nextRunResumed = await service.GetNextRunTime(1);
            Assert.NotNull(nextRunResumed);
            Assert.True(nextRunResumed > DateTime.Now);
        }

        [Fact]
        public async void ResumeJob_NotExistingJob()
        {
            var quartz = await SchedulerHelper.CreateScheduler();
            var service = SetupService(quartz);

            await service.ResumeJob(1);
        }
    }
}
