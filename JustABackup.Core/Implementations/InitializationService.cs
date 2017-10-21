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

        public async Task VerifyDatabase()
        {
            dbContext.Database.EnsureCreated();

            var providers = dbContext.Providers;
            foreach (var provider in providers)
                provider.IsProcessed = false;

            await dbContext.SaveChangesAsync();
        }

        public void LoadPlugins()
        {
            PreLoadAssembliesFromPath();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.DefinedTypes)
                {
                    if (type.ImplementedInterfaces.Contains(typeof(IBackupProvider)))
                        providerModelService.ProcessBackupProvider(type);

                    if (type.ImplementedInterfaces.Contains(typeof(IStorageProvider)))
                        providerModelService.ProcessStorageProvider(type);

                    if (type.ImplementedInterfaces.Contains(typeof(ITransformerProvider)))
                        providerModelService.ProcessTransformProvider(type);
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
    }
}
