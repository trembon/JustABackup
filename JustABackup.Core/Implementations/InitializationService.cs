using JustABackup.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using JustABackup.Database;
using JustABackup.Base;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace JustABackup.Core.Implementations
{
    public class InitializationService : IInitializationService
    {
        private DefaultContext dbContext;
        private IProviderModelService providerModelService;
        private IConfiguration configuration;

        public InitializationService(DefaultContext dbContext, IConfiguration configuration, IProviderModelService providerModelService)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
            this.providerModelService = providerModelService;
        }

        public async Task VerifyDatabase()
        {
            dbContext.Database.EnsureCreated();

            using (SqliteConnection quartzConnection = new SqliteConnection(configuration.GetConnectionString("quartz")))
            {
                await quartzConnection.OpenAsync();

                if(new FileInfo(quartzConnection.DataSource).Length == 0)
                {
                    string script = File.ReadAllText(configuration["Quartz:SchemaFile"]);

                    using (var command = quartzConnection.CreateCommand())
                    {
                        command.CommandText = script;
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task LoadPlugins()
        {
            PreLoadAssembliesFromPath();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.DefinedTypes)
                {
                    if (type.ImplementedInterfaces.Contains(typeof(IBackupProvider)))
                        await providerModelService.ProcessBackupProvider(type);

                    if (type.ImplementedInterfaces.Contains(typeof(IStorageProvider)))
                        await providerModelService.ProcessStorageProvider(type);

                    if (type.ImplementedInterfaces.Contains(typeof(ITransformerProvider)))
                        await providerModelService.ProcessTransformProvider(type);
                }
            }
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

        public Task VerifyScheduledJobs()
        {
            // TODO: verify that all backup jobs has a scheduled job with correct cron
            // TODO: pause all scheduled jobs with HasChangedModel on the backup jobs
            return Task.CompletedTask;
        }
    }
}
