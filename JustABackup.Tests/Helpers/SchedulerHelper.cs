using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Tests.Helpers
{
    internal static class SchedulerHelper
    {
        public async static Task<IScheduler> CreateScheduler()
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();

            // clears the scheduler, if we get a cached one
            await scheduler.Clear();
            
            var jobFactory = new TestableJobFactory();
            scheduler.JobFactory = jobFactory;

            return scheduler;
        }
    }
}
