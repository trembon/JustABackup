using JustABackup.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.ModelBinders
{
    // TODO: is this needed?
    public class ProviderPropertyModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            //if (context.Metadata.ModelType == typeof(ProviderPropertyModel))
            //    return new BinderTypeModelBinder(typeof(ProviderPropertyModelBinder));

            return null;
        }
    }
}
