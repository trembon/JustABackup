using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Text;
using Quartz;

namespace JustABackup.Tests.Helpers
{
    public class TestableJobFactory : IJobFactory
    {
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return null;
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }
    }
}
