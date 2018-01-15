using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JustABackup.Core.Services;
using Microsoft.EntityFrameworkCore;
using JustABackup.Database;
using JustABackup.Core.Extensions;
using JustABackup.Database.Repositories;
using JustABackup.Database.Helpers;

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
            services.AddScoped<IProviderMappingService, ProviderMappingService>();

            services.AddSingleton<ISchedulerService, SchedulerService>();

            // add repositories
            services.AddScoped<IAuthenticatedSessionRepository, AuthenticatedSessionRepository>();
            services.AddScoped<IBackupJobRepository, BackupJobRepository>();
            services.AddScoped<IProviderRepository, ProviderRepository>();
            services.AddScoped<IPassphraseRepository, PassphraseRepository>();
            services.AddScoped<IDatabaseEncryptionHelper, DatabaseEncryptionHelper>();

            // add quartz after all services
            services.AddQuartz(options =>
            {
                options.Add("quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz");
                options.Add("quartz.jobStore.useProperties", "true");
                options.Add("quartz.jobStore.dataSource", "default");
                options.Add("quartz.jobStore.tablePrefix", "QRTZ_");
                options.Add("quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.SQLiteDelegate, Quartz");
                options.Add("quartz.dataSource.default.provider", "SQLite-Microsoft");
                options.Add("quartz.dataSource.default.connectionString", Configuration.GetConnectionString("Quartz"));
                options.Add("quartz.serializer.type", "binary");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IInitializationService initializationService)
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
            await initializationService.LoadPlugins();
            await initializationService.VerifyScheduledJobs();

            app.UseQuartz();
        }
    }
}
