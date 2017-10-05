using JustABackup.Core.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JustABackup.Core.Implementations
{
    public class ProviderModelService : IProviderModelService
    {
        public void ProcessBackupProvider(Type type)
        {
            //Provider provider = new Provider();
            //provider.Name = type.Name;
            //provider.Namespace = type.AssemblyQualifiedName;
            //provider.Type = type.ImplementedInterfaces.Contains(typeof(IBackupProvider)) ? ProviderType.Backup : ProviderType.Storage;
            //provider.Version = type.Assembly.GetName().Version.ToString();
            //provider.Properties = GetProperties(type);

            //using (var context = new DefaultContext())
            //{
            //    context.Database.EnsureCreated();

            //    var existingProviders = context.Providers.ToDictionary(k => k.Namespace, v => v);

            //    foreach (var provider in providers)
            //    {
            //        if (existingProviders.ContainsKey(provider.Namespace))
            //        {
            //            // TODO: check version and update or validate

            //            existingProviders.Remove(provider.Namespace);
            //        }
            //        else
            //        {
            //            context.Add(provider);
            //        }
            //    }

            //    foreach (var kvp in existingProviders)
            //        context.Providers.Remove(kvp.Value); // TODO: dont remove, just flag as inactive

            //    context.SaveChanges();
            //}
        }

        public void ProcessStorageProvider(Type type)
        {
        }

        public void ProcessTransformProvider(Type type)
        {
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


        //private List<ProviderProperty> GetProperties(Type type)
        //{
        //    List<ProviderProperty> result = new List<ProviderProperty>();

        //    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //    foreach (var property in properties)
        //    {
        //        ProviderProperty providerProperty = new ProviderProperty();
        //        providerProperty.Name = property.Name;
        //        providerProperty.TypeName = property.Name;
        //        providerProperty.Type = GetTypeFromProperty(property);
        //        if (providerProperty.Type == -1)
        //            continue;

        //        var attributes = property.GetCustomAttributes(true);
        //        foreach (var attribute in attributes)
        //        {
        //            switch (attribute)
        //            {
        //                case DisplayAttribute da:
        //                    providerProperty.Name = da.Name;
        //                    providerProperty.Description = da.Description;
        //                    break;
        //            }
        //        }

        //        result.Add(providerProperty);
        //    }

        //    return result;
        //}
    }
}
