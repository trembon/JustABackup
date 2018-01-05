using JustABackup.Database.Enum;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JustABackup.Core.Services
{
    public interface ITypeMappingService
    {
        PropertyType GetTypeFromProperty(PropertyInfo property);

        string GetTemplateFromType(PropertyType type);

        object GetObjectFromString(string value, PropertyType propertyType);
    }

    public class TypeMappingService : ITypeMappingService
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

                default: return "String";
            }
        }

        public PropertyType GetTypeFromProperty(PropertyInfo property)
        {
            if (property.PropertyType == typeof(string))
                return PropertyType.String;

            if (property.PropertyType == typeof(int))
                return PropertyType.Number;

            if (property.PropertyType == typeof(long))
                return PropertyType.Number;

            if (property.PropertyType == typeof(bool))
                return PropertyType.Bool;

            throw new ArgumentOutOfRangeException(nameof(property));
        }
    }
}
