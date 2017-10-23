using Microsoft.AspNetCore.Builder;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Core.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        // TODO: make this work?
        //public static void UseQuartz(this IApplicationBuilder app, Action<Quartz> configuration)
        //{
        //    ISchedulerFactory sf = new StdSchedulerFactory(properties);

        //    scheduler = await sf.GetScheduler();
        //    await scheduler.Start();

        //    var jobFactory = (IJobFactory)app.ApplicationServices.GetService(typeof(IJobFactory));
        //    Quartz.Instance.UseJobFactory(jobFactory);

        //    // Run configuration
        //    configuration.Invoke(Quartz.Instance);
        //    // Run Quartz
        //    Quartz.Start();
        //}
    }

    //public static class QuartzExtensions
    //{
    //    public static void UseQuartz(this IApplicationBuilder app)
    //    {
    //        app.ApplicationServices.GetService<IScheduler>();
    //    }

    //    public static async void AddQuartz(this IServiceCollection services)
    //    {
    //        var props = new NameValueCollection
    //    {
    //        {"quartz.serializer.type", "json"}
    //    };
    //        var factory = new StdSchedulerFactory(props);
    //        var scheduler = await factory.GetScheduler();

    //        var jobFactory = new IoCJobFactory(services.BuildServiceProvider());
    //        scheduler.JobFactory = jobFactory;
    //        await scheduler.Start();
    //        services.AddSingleton(scheduler);
    //    }
    //}
}
