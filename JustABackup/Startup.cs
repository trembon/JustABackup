using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JustABackup.Base;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.IO;
using JustABackup.Core.Services;
using JustABackup.Core.Implementations;
using Microsoft.EntityFrameworkCore;
using JustABackup.Database;
using System.Collections.Specialized;
using Quartz;
using Quartz.Impl;

namespace JustABackup
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddSession();

            // register database context
            services.AddDbContext<DefaultContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("Default"))
            );

            // register services
            services.AddScoped<IInitializationService, InitializationService>();
            services.AddScoped<IProviderModelService, ProviderModelService>();
            services.AddSingleton<ISchedulerService, SchedulerService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IInitializationService initializationService, ISchedulerService schedulerService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSession();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            await initializationService.VerifyDatabase();
            initializationService.LoadPlugins();
            
            var properties = new NameValueCollection
            {
                ["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz",
                ["quartz.jobStore.useProperties"] = "true",
                ["quartz.jobStore.dataSource"] = "default",
                ["quartz.jobStore.tablePrefix"] = "QRTZ_",
                ["quartz.jobStore.driverDelegateType"] = "Quartz.Impl.AdoJobStore.SQLiteDelegate, Quartz",
                ["quartz.dataSource.default.provider"] = "SQLite-Microsoft",
                ["quartz.dataSource.default.connectionString"] = "Data Source=quartz.sqlite",
                ["quartz.serializer.type"] = "binary"
            };
            await schedulerService.CreateScheduler(properties, app.ApplicationServices);
        }
    }
}
