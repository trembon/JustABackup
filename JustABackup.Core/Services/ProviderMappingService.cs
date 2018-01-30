using JustABackup.Base;
using JustABackup.Core.Entities;
using JustABackup.Database;
using JustABackup.Database.Entities;
using JustABackup.Database.Enum;
using JustABackup.Database.Repositories;
using JustABackup.Models.Job;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.Services
{
    public interface IProviderMappingService
    {
        Task<T> CreateProvider<T>(int providerInstanceId) where T : class;

        Task<T> CreateProvider<T>(ProviderInstance providerInstance) where T : class;

        string GetTemplateFromType(PropertyType type);
        
        Task<object> GetValue(ProviderInstanceProperty property);

        Task<object> GetPresentationValue(ProviderInstanceProperty property);

        Task<byte[]> Parse(string value, PropertyType propertyType, List<ProviderPropertyAttribute> attributes);

        Task<ProviderInstance> CreateProviderInstance(Provider provider, CreateProviderModel createProviderModel);
    }

    public class ProviderMappingService : IProviderMappingService
    {
        private IEncryptionService encryptionService;
        private IProviderRepository providerRepository;
        private IAuthenticatedSessionRepository authenticatedSessionRepository;

        public ProviderMappingService(IProviderRepository providerRepository, IAuthenticatedSessionRepository authenticatedSessionRepository, IEncryptionService encryptionService)
        {
            this.encryptionService = encryptionService;
            this.providerRepository = providerRepository;
            this.authenticatedSessionRepository = authenticatedSessionRepository;
        }

        public async Task<T> CreateProvider<T>(int providerInstanceId) where T : class
        {
            ProviderInstance providerInstance = await providerRepository.GetInstance(providerInstanceId);
			return await CreateProvider<T>(providerInstance);
        }

        public async Task<T> CreateProvider<T>(ProviderInstance providerInstance) where T : class
        {
            Type providerType = Type.GetType(providerInstance.Provider.Namespace);
            T convertedProvider = Activator.CreateInstance(providerType) as T;

            foreach (var property in providerInstance.Values)
            {
                PropertyInfo propertyInfo = providerType.GetProperty(property.Property.TypeName);
                
                object originalValueType = await this.GetValue(property);
                propertyInfo.SetValue(convertedProvider, originalValueType);
            }

            return convertedProvider;
        }

        public string GetTemplateFromType(PropertyType type)
        {
            switch (type)
            {
                case PropertyType.String: return "String";
                case PropertyType.Number: return "Number";
                case PropertyType.Bool: return "Boolean";
                case PropertyType.Authentication: return "AuthenticationProvider";

                default: return "String";
            }
        }

        public async Task<object> GetValue(ProviderInstanceProperty property)
        {
            object value = await encryptionService.Decrypt<object>(property.Value);

            switch (property.Property.Type)
            {
                case PropertyType.Authentication:
                    ((dynamic)value).SetLoadMethod((Func<int, Task<object>>)GetAuthenticatedSessionClient);
                    break;
            }

            List<ProviderPropertyAttribute> attributes = property?.Property?.Attributes ?? new List<ProviderPropertyAttribute>();
            if (attributes.Any(a => a.Name == PropertyAttribute.Transform))
            {
                switch (property.Property.Type)
                {
                    case PropertyType.String:
                        value = TransformString(value);
                        break;
                }
            }

            return value;
        }

        public async Task<object> GetPresentationValue(ProviderInstanceProperty property)
        {
            return await encryptionService.Decrypt<object>(property.Value);
        }

        public async Task<byte[]> Parse(string value, PropertyType propertyType, List<ProviderPropertyAttribute> attributes)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new byte[0];

            attributes = attributes ?? new List<ProviderPropertyAttribute>();
            string genericType = attributes.FirstOrDefault(a => a.Name == PropertyAttribute.GenericParameter)?.Value;

            object parsedValue;
            switch (propertyType)
            {
                case PropertyType.Bool:
                    parsedValue = Convert.ToBoolean(value);
                    break;

                case PropertyType.Number:
                    parsedValue = Convert.ToInt64(value);
                    break;

                case PropertyType.Authentication:
                    Type authenticatedClientType = typeof(AuthenticatedClient<>);
                    authenticatedClientType = authenticatedClientType.MakeGenericType(Type.GetType(genericType));
                    parsedValue = Activator.CreateInstance(authenticatedClientType, Convert.ToInt32(value));

                    ((dynamic)parsedValue).SetLoadMethod((Func<int, Task<object>>)GetAuthenticatedSessionClient);
                    break;

                default:
                case PropertyType.String:
                    parsedValue = value;
                    break;
            }

            byte[] encryptedValue = await encryptionService.Encrypt(parsedValue);
            return encryptedValue;
        }

        private object TransformString(object value)
        {
            if (value is string)
            {
                string data = value as string;
                DateTime timestamp = DateTime.Now;

                data = data.Replace("{date}", timestamp.ToShortDateString());

                return data;
            }

            return value;
        }

        private async Task<object> GetAuthenticatedSessionClient(int authenticatedSessionId)
        {
            AuthenticatedSession session = await authenticatedSessionRepository.Get(authenticatedSessionId);
            string decryptedSessionData = await encryptionService.Decrypt<string>(session.SessionData);

            IAuthenticationProvider<object> authenticationProvider = await CreateProvider<IAuthenticationProvider<object>>(session.Provider.ID);
            return authenticationProvider.GetAuthenticatedClient(decryptedSessionData);
        }

        public async Task<ProviderInstance> CreateProviderInstance(Provider provider, CreateProviderModel createProviderModel)
        {
            ProviderInstance backupProvider = new ProviderInstance();
            backupProvider.Provider = provider;
            foreach (var property in createProviderModel.Properties)
            {
                ProviderInstanceProperty instanceProperty = new ProviderInstanceProperty();
                instanceProperty.Property = provider.Properties.FirstOrDefault(p => p.Name == property.Name);
                instanceProperty.Value = await Parse(property.Value.ToString(), instanceProperty.Property.Type, instanceProperty.Property.Attributes);
                backupProvider.Values.Add(instanceProperty);
            }
            return backupProvider;
        }
    }
}
