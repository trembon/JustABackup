using JustABackup.Base;
using JustABackup.Database;
using JustABackup.Database.Contexts;
using JustABackup.Database.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private IConfiguration configuration;
        private ISchedulerService schedulerService;
        private ILogger<InitializationService> logger;
        private IProviderModelService providerModelService;

        public InitializationService(DefaultContext dbContext, IConfiguration configuration, IProviderModelService providerModelService, ISchedulerService schedulerService, ILogger<InitializationService> logger)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this.configuration = configuration;
            this.schedulerService = schedulerService;
            this.providerModelService = providerModelService;
        }

        public async Task VerifyDatabase()
        {
            try
            {
                dbContext.Database.EnsureCreated();

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
            catch(Exception ex)
            {
                logger.LogError(ex, $"Failed to verify and create databases");
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
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to load and find providers (plugins).");
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
