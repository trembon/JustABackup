using JustABackup.Core.ScheduledJobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace JustABackup.Core.Extensions
{
    public static class QuartzExtensions
    {
        public static async void UseQuartz(this IApplicationBuilder app)
        {
            IScheduler scheduler = app.ApplicationServices.GetService<IScheduler>();
            await scheduler.Start();
        }

        public static void AddQuartz(this IServiceCollection services)
        {
            AddQuartz(services, null);
        }

        public static async void AddQuartz(this IServiceCollection services, Action<NameValueCollection> configuration)
        {
            var props = new NameValueCollection();

            configuration?.Invoke(props);

            ISchedulerFactory factory = new StdSchedulerFactory(props);
            IScheduler scheduler = await factory.GetScheduler();
            
            services.AddSingleton(scheduler);

            services.AddTransient<DefaultScheduledJob>();

            var jobFactory = new JobFactory(services.BuildServiceProvider());
            scheduler.JobFactory = jobFactory;
        }
    }
}
