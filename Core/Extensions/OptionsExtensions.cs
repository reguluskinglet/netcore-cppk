using Core.Grid.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Core.Extensions
{
    public static class OptionsExtensions
    {
        public static void UseGridOptionsModelBinding(this MvcOptions options, IMemoryCache cache)
        {
            options.ModelBinderProviders.Insert(0, new GridOptionsModelBinderProvider(cache));
        }
    }
}
