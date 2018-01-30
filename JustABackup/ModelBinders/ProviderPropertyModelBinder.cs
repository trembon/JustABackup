using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.ModelBinders
{
    public class ProviderPropertyModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            object model = Activator.CreateInstance(bindingContext.ModelType);

            bool allNull = true;
            foreach(var property in bindingContext.ModelType.GetProperties())
            {
                string modelNameProperty = $"{bindingContext.ModelName}.{property.Name}";

                ValueProviderResult value = bindingContext.ValueProvider.GetValue(modelNameProperty);
                if (value != ValueProviderResult.None)
                {
                    allNull = false;
                    bindingContext.ModelState.SetModelValue(modelNameProperty, value);
                    property.SetValue(model, value.FirstValue);
                }
            }

            if (!allNull)
                bindingContext.Result = ModelBindingResult.Success(model);

            return Task.CompletedTask;
        }
    }
}
