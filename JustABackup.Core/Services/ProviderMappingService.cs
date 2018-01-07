using JustABackup.Base;
using JustABackup.Core.Entities;
using JustABackup.Database.Enum;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JustABackup.Core.Services
{
    public interface IProviderMappingService
    {
        PropertyType GetTypeFromProperty(PropertyInfo property, out Type genericParameter);

        string GetTemplateFromType(PropertyType type);

        object GetObjectFromString(string value, PropertyType propertyType);
    }

    public class ProviderMappingService : IProviderMappingService
    {
        public object GetObjectFromString(string value, PropertyType propertyType)
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
                    return new AuthenticatedClient<object>(Convert.ToInt64(value));
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
