using JustABackup.Database;
using JustABackup.Database.Entities;
using JustABackup.Database.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.Services
{
    public interface IProviderModelService
    {
        Task ProcessBackupProvider(Type type);

        Task ProcessStorageProvider(Type type);

        Task ProcessTransformProvider(Type type);

        Task ProcessAuthenticationProvider(Type type, Type interfaceType);
    }

    public class ProviderModelService : IProviderModelService
    {
        private DefaultContext dbContext;
        private IProviderMappingService typeMappingService;

        public ProviderModelService(DefaultContext dbContext, IProviderMappingService typeMappingService)
        {
            this.dbContext = dbContext;
            this.typeMappingService = typeMappingService;
        }

        public async Task ProcessBackupProvider(Type type)
        {
            await ProcessProvider(type, null, ProviderType.Backup);
        }

        public async Task ProcessStorageProvider(Type type)
        {
            await ProcessProvider(type, null, ProviderType.Storage);
        }

        public async Task ProcessTransformProvider(Type type)
        {
            await ProcessProvider(type, null, ProviderType.Transform);
        }

        public async Task ProcessAuthenticationProvider(Type type, Type interfaceType)
        {
            await ProcessProvider(type, interfaceType.GenericTypeArguments[0], ProviderType.Authentication);
        }

        private async Task ProcessProvider(Type type, Type genericType, ProviderType providerType)
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
            provider.GenericType = genericType?.AssemblyQualifiedName;
            provider.Properties = GetProperties(type);

            var existingProvider = dbContext.Providers.Include(p => p.Properties).FirstOrDefault(p => p.FullName.Equals(provider.FullName));
            if (existingProvider != null)
            {
                bool modelHasChanged = false;

                // check if the metadata of the provider has changed
                if (!existingProvider.Equals(provider, false))
                {
                    if (existingProvider.Type != provider.Type)
                        modelHasChanged = true;

                    existingProvider.Name = provider.Name;
                    existingProvider.FullName = provider.FullName;
                    existingProvider.Namespace = provider.Namespace;
                    existingProvider.Version = provider.Version;
                    existingProvider.Type = provider.Type;
                }

                // check if the properties of the provider has changed
                // remove non longer existing properties
                for (int i = existingProvider.Properties.Count - 1; i >= 0; i--)
                {
                    ProviderProperty providerProperty = provider.Properties.FirstOrDefault(p => p.TypeName == existingProvider.Properties[i].TypeName);
                    if (providerProperty == null)
                    {
                        existingProvider.Properties.RemoveAt(i);
                        modelHasChanged = true;
                    }
                }

                // add or update properties
                foreach (var property in provider.Properties)
                {
                    ProviderProperty existingProperty = existingProvider.Properties.FirstOrDefault(p => p.TypeName == property.TypeName);
                    if (existingProperty == null)
                    {
                        existingProvider.Properties.Add(property);
                        modelHasChanged = true;
                    }
                    else if (!existingProperty.Equals(property, false))
                    {
                        existingProperty.Name = property.Name;
                        existingProperty.TypeName = property.TypeName;
                        existingProperty.Type = property.Type;
                        existingProperty.Description = property.Description;

                        existingProperty.Attributes.Clear();
                        existingProperty.Attributes = property.Attributes;

                        modelHasChanged = true;
                    }
                }

                if (modelHasChanged)
                {
                    // find all jobs that uses the changed model
                    var jobsWithChangedModels = dbContext
                        .Jobs
                        .Include(j => j.Providers)
                        .Where(j => j.Providers.Any(t => t.ID == existingProvider.ID));

                    // TODO: handle authenticated sessions

                    // set the jobs to have a changed model
                    foreach (BackupJob job in jobsWithChangedModels)
                        job.HasChangedModel = true;
                }
            }
            else
            {
                await dbContext.Providers.AddAsync(provider);
            }

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch
            {
                // TODO: log
            }
        }

        private List<ProviderProperty> GetProperties(Type type)
        {
            List<ProviderProperty> result = new List<ProviderProperty>();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                try
                {
                    ProviderProperty providerProperty = new ProviderProperty();
                    providerProperty.Name = property.Name;
                    providerProperty.TypeName = property.Name;
                    providerProperty.Type = typeMappingService.GetTypeFromProperty(property, out Type genericParameter);

                    if (genericParameter != null)
                        providerProperty.Attributes.Add(new ProviderPropertyAttribute(PropertyAttribute.GenericParameter, genericParameter.AssemblyQualifiedName));

                    var attributes = property.GetCustomAttributes(true);
                    foreach (var attribute in attributes)
                    {
                        switch (attribute)
                        {
                            case DisplayAttribute da:
                                providerProperty.Name = da.Name;
                                providerProperty.Description = da.Description;
                                break;

                            case PasswordPropertyTextAttribute ppta:
                                providerProperty.Attributes.Add(new ProviderPropertyAttribute(PropertyAttribute.Password, bool.TrueString));
                                    break;
                        }
                    }

                    result.Add(providerProperty);
                }
                catch { }
            }

            return result;
        }
    }
}
