using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models
{
    public class ProviderPropertyModel
    {
        public string Name { get; set; }

        public string Description { get; set; }
        
        public object Value { get; set; }

        public string Template { get; set; }

        public RouteValueDictionary ViewData { get; set; }

        public ProviderPropertyModel()
        {
        }

        public ProviderPropertyModel(string name, string description, string template, object value = null, Dictionary<string, string> viewData = null)
        {
            this.Name = name;
            this.Description = description;
            this.Template = template;
            this.Value = value;
            
            this.ViewData = new RouteValueDictionary(viewData);
        }
    }
}
