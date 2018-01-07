using JustABackup.Database.Entities;
using JustABackup.Database.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
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

        public object ViewData { get; set; }
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
