using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Memory;

namespace Core.Grid.ModelBinders
{
    public class GridOptionsModelBinderProvider : IModelBinderProvider
    {
        private readonly IMemoryCache _cache;

        public GridOptionsModelBinderProvider(IMemoryCache cache)
        {
            _cache = cache;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.IsComplexType &&  typeof(IGridOptions).IsAssignableFrom(context.Metadata.ModelType))
            {
                var propertyBinders = context.Metadata.Properties.ToDictionary(property => property, context.CreateBinder);

                return new GridOptionsModelBinder(propertyBinders, _cache);
            }

            return null;
        }
    }
}
