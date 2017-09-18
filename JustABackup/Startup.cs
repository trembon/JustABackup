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

namespace JustABackup
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            PreLoadAssembliesFromPath();
            LoadPlugins();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

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
        }

        private void PreLoadAssembliesFromPath()
        {
            FileInfo[] files = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).GetFiles("*.dll", SearchOption.AllDirectories);
            
            foreach (var fi in files)
            {
                string s = fi.FullName;
                AssemblyName a = AssemblyName.GetAssemblyName(s);
                if (!AppDomain.CurrentDomain.GetAssemblies().Any(assembly => AssemblyName.ReferenceMatchesDefinition(a, assembly.GetName())))
                {
                    Assembly.Load(a);
                }
            }
        }

        private void LoadPlugins()
        {
            List<Provider> providers = new List<Provider>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in assemblies)
            {
                foreach(var type in assembly.DefinedTypes)
                {
                    if (type.ImplementedInterfaces.Contains(typeof(IBackupProvider)) || type.ImplementedInterfaces.Contains(typeof(IStorageProvider)))
                    {
                        Provider provider = new Provider();
                        provider.Name = type.Name;
                        provider.Namespace = type.AssemblyQualifiedName;
                        provider.Type = type.ImplementedInterfaces.Contains(typeof(IBackupProvider)) ? ProviderType.Backup : ProviderType.Storage;
                        provider.Version = type.Assembly.GetName().Version.ToString();
                        provider.Properties = GetProperties(type);

                        providers.Add(provider);
                    }
                }
            }

            using (var context = new DefaultContext())
            {
                context.Database.EnsureCreated();

                var existingProviders = context.Providers.ToDictionary(k => k.Namespace, v => v);

                foreach(var provider in providers)
                {
                    if (existingProviders.ContainsKey(provider.Namespace))
                    {
                        // TODO: check version and update or validate

                        existingProviders.Remove(provider.Namespace);
                    }
                    else
                    {
                        context.Add(provider);
                    }
                }

                foreach (var kvp in existingProviders)
                    context.Providers.Remove(kvp.Value); // TODO: dont remove, just flag as inactive

                context.SaveChanges();
            }
        }

        private List<ProviderProperty> GetProperties(Type type)
        {
            List<ProviderProperty> result = new List<ProviderProperty>();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach(var property in properties)
            {
                ProviderProperty providerProperty = new ProviderProperty();
                providerProperty.Name = property.Name;
                providerProperty.TypeName = property.Name;
                providerProperty.Type = GetTypeFromProperty(property);
                if (providerProperty.Type == -1)
                    continue;

                var attributes = property.GetCustomAttributes(true);
                foreach(var attribute in attributes)
                {
                    switch (attribute)
                    {
                        case DisplayAttribute da:
                            providerProperty.Name = da.Name;
                            providerProperty.Description = da.Description;
                            break;
                    }
                }

                result.Add(providerProperty);
            }

            return result;
        }

        private int GetTypeFromProperty(PropertyInfo property)
        {
            if (property.PropertyType == typeof(string))
                return 0;

            if (property.PropertyType == typeof(int))
                return 1;

            if (property.PropertyType == typeof(bool))
                return 2;

            return -1;
        }
    }
}
