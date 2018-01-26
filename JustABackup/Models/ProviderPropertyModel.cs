using JustABackup.Core.Services;
using JustABackup.Database.Entities;
using JustABackup.Database.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

        [ModelBinder(BinderType = typeof(ProviderPropertyValueModelBinder))]
        public object Value { get; set; }

        public string Template { get; set; }

        public RouteValueDictionary ViewData { get; set; }

        public ProviderPropertyModel()
        {
        }

        public ProviderPropertyModel(ProviderProperty providerProperty, object value, IProviderMappingService providerMappingService)
        {
            this.Name = providerProperty.Name;
            this.Description = providerProperty.Description;
            this.Template = providerMappingService.GetTemplateFromType(providerProperty.Type);
            this.Value = value;
            
            ViewData = new RouteValueDictionary();
            foreach (var attribute in providerProperty.Attributes)
                ViewData.Add(attribute.Name.ToString(), attribute.Value);
        }
    }

    public class ProviderPropertyValueModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));
            
            ValueProviderResult value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);

            bindingContext.Result = ModelBindingResult.Success(value.FirstValue);
            return Task.CompletedTask;
        }
    }
}
