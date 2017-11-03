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

namespace JustABackup.Core.Implementations
{
    public class InitializationService : IInitializationService
    {
        private DefaultContext dbContext;
        private IProviderModelService providerModelService;

        public InitializationService(DefaultContext dbContext, IProviderModelService providerModelService)
        {
            this.dbContext = dbContext;
            this.providerModelService = providerModelService;
        }

        public Task VerifyDatabase()
        {
            dbContext.Database.EnsureCreated();

            // TODO: create quartz db if needed

            return Task.CompletedTask;
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
