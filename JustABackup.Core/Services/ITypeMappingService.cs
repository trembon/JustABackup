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
}
