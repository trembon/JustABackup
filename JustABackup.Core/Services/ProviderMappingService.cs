using JustABackup.Base;
using JustABackup.Core.Entities;
using JustABackup.Database;
using JustABackup.Database.Entities;
using JustABackup.Database.Enum;
using JustABackup.Database.Repositories;
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

        PropertyType GetTypeFromProperty(PropertyInfo property, out Type genericParameter);

        string GetTemplateFromType(PropertyType type);

        object GetObjectFromString(string value, PropertyType propertyType, string genericType = null);
    }

    public class ProviderMappingService : IProviderMappingService
    {
        private IProviderRepository providerRepository;
        private IAuthenticatedSessionRepository authenticatedSessionRepository;

        public ProviderMappingService(IProviderRepository providerRepository, IAuthenticatedSessionRepository authenticatedSessionRepository)
        {
            this.providerRepository = providerRepository;
        }

        public async Task<T> CreateProvider<T>(int providerInstanceId) where T : class
        {
            ProviderInstance providerInstance = await providerRepository.GetInstance(providerInstanceId);
			return await CreateProvider<T>(providerInstance);
        }

        public Task<T> CreateProvider<T>(ProviderInstance providerInstance) where T : class
        {
            Type providerType = Type.GetType(providerInstance.Provider.Namespace);
            T convertedProvider = Activator.CreateInstance(providerType) as T;

            foreach (var property in providerInstance.Values)
            {
                PropertyInfo propertyInfo = providerType.GetProperty(property.Property.TypeName);

                string genericType = property.Property.Attributes.Where(a => a.Name == PropertyAttribute.GenericParameter).Select(a => a.Value).FirstOrDefault();
                object originalValueType = this.GetObjectFromString(property.Value, property.Property.Type, genericType);

                propertyInfo.SetValue(convertedProvider, originalValueType);
            }

            return Task.FromResult(convertedProvider);
        }

        public object GetObjectFromString(string value, PropertyType propertyType, string genericType = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            switch (propertyType)
            {
                case PropertyType.Bool:
                    return Convert.ToBoolean(value);

                case PropertyType.Number:
                    return Convert.ToInt64(value);

                case PropertyType.String:
                    return value;

                case PropertyType.Authentication:
                    if (string.IsNullOrWhiteSpace(genericType))
                    {
                        return new AuthenticatedClient<object>(Convert.ToInt32(value), this, authenticatedSessionRepository);
                    }
                    else
                    {
                        Type authenticatedClientType = typeof(AuthenticatedClient<>);
                        authenticatedClientType = authenticatedClientType.MakeGenericType(Type.GetType(genericType));
                        return Activator.CreateInstance(authenticatedClientType, Convert.ToInt64(value), this, authenticatedSessionRepository);
                    }
            }

            return null;
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

        public PropertyType GetTypeFromProperty(PropertyInfo property, out Type genericParameter)
        {
            genericParameter = null;

            if (property.PropertyType == typeof(string))
                return PropertyType.String;

            if (property.PropertyType == typeof(int))
                return PropertyType.Number;

            if (property.PropertyType == typeof(long))
                return PropertyType.Number;

            if (property.PropertyType == typeof(bool))
                return PropertyType.Bool;

            if (property.PropertyType.GetGenericTypeDefinition() == typeof(IAuthenticatedClient<>))
            {
                genericParameter = property.PropertyType.GenericTypeArguments[0];
                return PropertyType.Authentication;
            }

            throw new ArgumentOutOfRangeException(nameof(property));
        }
    }
}
