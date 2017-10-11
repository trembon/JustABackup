using JustABackup.Core.Contexts;
using JustABackup.Core.Entities;
using JustABackup.Core.Entities.Database;
using JustABackup.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.Implementations
{
    public class ProviderModelService : IProviderModelService
    {
        private DefaultContext defaultContext;

        public ProviderModelService(DefaultContext defaultContext)
        {
            this.defaultContext = defaultContext;
        }

        public async Task ProcessBackupProvider(Type type)
        {
            await ProcessProvider(type, ProviderType.Backup);
        }

        public async Task ProcessStorageProvider(Type type)
        {
            await ProcessProvider(type, ProviderType.Storage);
        }

        public async Task ProcessTransformProvider(Type type)
        {
            await ProcessProvider(type, ProviderType.Transform);
        }

        private async Task ProcessProvider(Type type, ProviderType providerType)
        {
            Provider provider = new Provider();

            DisplayNameAttribute displayAttribute = type.GetCustomAttribute<DisplayNameAttribute>();
            if (displayAttribute != null)
            {
                provider.Name = displayAttribute.DisplayName;
            }
            else
            {
                provider.Name = type.Name;
            }

            provider.FullName = type.FullName;
            provider.Namespace = type.AssemblyQualifiedName;
            provider.Type = providerType;
            provider.Version = type.Assembly.GetName().Version.ToString();
            provider.Properties = GetProperties(type);
            provider.IsProcessed = true;

            var existingProvider = defaultContext.Providers.FirstOrDefault(p => p.FullName.Equals(provider.FullName));
            if(existingProvider != null)
            {
                // TODO: update provider and mark as changed in existing jobs
            }
            else
            {
                await defaultContext.Providers.AddAsync(provider);
            }

            await defaultContext.SaveChangesAsync();
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
        
        private List<ProviderProperty> GetProperties(Type type)
        {
            List<ProviderProperty> result = new List<ProviderProperty>();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                ProviderProperty providerProperty = new ProviderProperty();
                providerProperty.Name = property.Name;
                providerProperty.TypeName = property.Name;
                providerProperty.Type = GetTypeFromProperty(property);
                if (providerProperty.Type == -1)
                    continue;

                var attributes = property.GetCustomAttributes(true);
                foreach (var attribute in attributes)
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
    }
}
