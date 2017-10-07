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
using JustABackup.DAL.Entities;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using JustABackup.DAL.Contexts;
using System.IO;
using JustABackup.Core.Services;
using JustABackup.Core.Implementations;
using Microsoft.EntityFrameworkCore;

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
                options.UseSqlite("Data Source=demo.db")
            );

            // register services
            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddSingleton<IPluginService, PluginService>();
            services.AddSingleton<IProviderModelService, ProviderModelService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IDatabaseService databaseService)
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

            databaseService.VerifyDatabase();

            //app.ApplicationServices.GetService<IDatabaseService>().VerifyDatabase();
            //app.ApplicationServices.GetService<IPluginService>().LoadPlugins();
        }
    }
}
