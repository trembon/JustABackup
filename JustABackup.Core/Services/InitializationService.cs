using JustABackup.Base;
using JustABackup.Database;
using JustABackup.Database.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.Services
{
    public interface IInitializationService
    {
        Task VerifyDatabase();

        Task VerifyScheduledJobs();

        Task LoadPlugins();
    }

    public class InitializationService : IInitializationService
    {
        private DefaultContext dbContext;
        private IProviderModelService providerModelService;
        private IConfiguration configuration;
        private ISchedulerService schedulerService;

        public InitializationService(DefaultContext dbContext, IConfiguration configuration, IProviderModelService providerModelService, ISchedulerService schedulerService)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
            this.providerModelService = providerModelService;
            this.schedulerService = schedulerService;
        }

        public async Task VerifyDatabase()
        {
            dbContext.Database.EnsureCreated();

            try
            {
                using (SqliteConnection quartzConnection = new SqliteConnection(configuration.GetConnectionString("quartz")))
                {
                    await quartzConnection.OpenAsync();

                    if (new FileInfo(quartzConnection.DataSource).Length == 0)
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
            catch
            {
                // TODO: log
            }
        }

        public async Task LoadPlugins()
        {
            PreLoadAssembliesFromPath();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (TypeInfo type in assembly.DefinedTypes)
                {
                    Type authenticationInterface = type.ImplementedInterfaces.Where(i => i.IsGenericType).FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IAuthenticationProvider<>));
                    if (authenticationInterface != null)
                        await providerModelService.ProcessAuthenticationProvider(type, authenticationInterface);

                    if (type.ImplementedInterfaces.Contains(typeof(IBackupProvider)))
                        await providerModelService.ProcessBackupProvider(type);

                    if (type.ImplementedInterfaces.Contains(typeof(IStorageProvider)))
                        await providerModelService.ProcessStorageProvider(type);

                    if (type.ImplementedInterfaces.Contains(typeof(ITransformProvider)))
                        await providerModelService.ProcessTransformProvider(type);
                }
            }
        }

        private void PreLoadAssembliesFromPath()
        {
            FileInfo[] files = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).GetFiles("*.dll", SearchOption.AllDirectories);

            foreach (var fi in files)
            {
                try
                {
                    string s = fi.FullName;
                    AssemblyName a = AssemblyName.GetAssemblyName(s);
                    if (!AppDomain.CurrentDomain.GetAssemblies().Any(assembly => AssemblyName.ReferenceMatchesDefinition(a, assembly.GetName())))
                    {
                        Assembly.Load(a);
                    }
                }
                catch
                {
                    // TODO: log
                }
            }
        }

        public async Task VerifyScheduledJobs()
        {
            foreach (BackupJob job in dbContext.Jobs)
            {
                if (job.HasChangedModel)
                {
                    await schedulerService.PauseJob(job.ID);
                }
            }
        }
    }
}
