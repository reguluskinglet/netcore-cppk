using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Caching.Memory;

namespace Core.Grid.ModelBinders
{
    public class GridOptionsModelBinder : ComplexTypeModelBinder
    {
        private readonly IMemoryCache _memoryCache;

        public GridOptionsModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders, IMemoryCache memoryCache)
            : base(propertyBinders)
        {
            _memoryCache = memoryCache;
        }

        protected override object CreateModel(ModelBindingContext bindingContext)
        {
            IGridOptions options;

            string key = bindingContext.HttpContext.User.Identity.Name + bindingContext.ModelType.Name;

            if (!_memoryCache.TryGetValue(key, out options))
            {
                options = Activator.CreateInstance(bindingContext.ModelType) as IGridOptions;

                _memoryCache.Set(key, options, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(365)));
            }

            if (bindingContext.HttpContext.Request.ContentLength > 0)
                options.Page = 1;

            return options;
        }

        public static string GridKey(ActionContext actioncontext)
        {
           
            return actioncontext.ActionDescriptor.Id;
        }
    }
}
