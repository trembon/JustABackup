﻿using JustABackup.Core.Services;
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

namespace JustABackup.Core.Implementations
{
    public class ProviderModelService : IProviderModelService
    {
        private DefaultContext dbContext;
        private ITypeMappingService typeMappingService;

        public ProviderModelService(DefaultContext dbContext, ITypeMappingService typeMappingService)
        {
            this.dbContext = dbContext;
            this.typeMappingService = typeMappingService;
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

            var existingProvider = dbContext.Providers.Include(p => p.Properties).FirstOrDefault(p => p.FullName.Equals(provider.FullName));
            if(existingProvider != null)
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
                for(int i = existingProvider.Properties.Count - 1; i >= 0; i--)
                {
                    ProviderProperty providerProperty = provider.Properties.FirstOrDefault(p => p.TypeName == existingProvider.Properties[i].TypeName);
                    if(providerProperty == null)
                    {
                        existingProvider.Properties.RemoveAt(i);
                        modelHasChanged = true;
                    }
                }
                
                // add or update properties
                foreach(var property in provider.Properties)
                {
                    ProviderProperty existingProperty = existingProvider.Properties.FirstOrDefault(p => p.TypeName == property.TypeName);
                    if(existingProperty == null)
                    {
                        existingProvider.Properties.Add(property);
                        modelHasChanged = true;
                    }
                    else if(!existingProperty.Equals(property, false))
                    {
                        existingProperty.Name = property.Name;
                        existingProperty.TypeName = property.TypeName;
                        existingProperty.Type = property.Type;
                        existingProperty.Description = property.Description;

                        modelHasChanged = true;
                    }
                }

                if (modelHasChanged)
                {
                    var jobsWithChangedModels = dbContext
                        .Jobs
                        .Include(j => j.TransformProviders)
                        .Where(j => j.StorageProvider.ID == existingProvider.ID || j.BackupProvider.ID == existingProvider.ID || j.TransformProviders.Any(t => t.ID == existingProvider.ID));

                    foreach(BackupJob job in jobsWithChangedModels)
                        job.HasChangedModel = true;
                }
            }
            else
            {
                await dbContext.Providers.AddAsync(provider);
            }

            await dbContext.SaveChangesAsync();
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
                    providerProperty.Type = typeMappingService.GetTypeFromProperty(property);

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
                catch { }
            }

            return result;
        }
    }
}
