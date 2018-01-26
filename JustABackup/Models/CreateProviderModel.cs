using JustABackup.Core.Services;
using JustABackup.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models
{
    public class CreateProviderModel : BaseViewModel
    {
        public string ProviderName { get; set; }

        public List<ProviderPropertyModel> Properties { get; set; }

        // TODO: place in service
        public async Task<ProviderInstance> CreateProviderInstance(Provider provider, IProviderMappingService providerMappingService)
        {
            ProviderInstance backupProvider = new ProviderInstance();
            backupProvider.Provider = provider;
            foreach (var property in Properties)
            {
                ProviderInstanceProperty instanceProperty = new ProviderInstanceProperty();
                instanceProperty.Property = provider.Properties.FirstOrDefault(p => p.Name == property.Name);
                instanceProperty.Value = await providerMappingService.Parse(property.Value.ToString(), instanceProperty.Property.Type, instanceProperty.Property.Attributes);
                backupProvider.Values.Add(instanceProperty);
            }
            return backupProvider;
        }
    }
}
