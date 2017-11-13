using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Core.Extenssions
{
    public static class ISessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            string serializedValue = JsonConvert.SerializeObject(value);
            session.SetString(key, serializedValue);
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            string serializedValue = session.GetString("CreateJob");
            if (string.IsNullOrWhiteSpace(serializedValue))
                return default(T);

            return JsonConvert.DeserializeObject<T>(serializedValue);
        }
    }
}
