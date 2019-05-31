using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Rzdppk.Core.Extensions
{
    public static class JsonExtension
    {
        public static TResult ToJson<TResult>(this Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                return JsonConvert.DeserializeObject<TResult>(reader.ReadToEnd());
            }
        }

        public static string ToJson<TEntity>(this TEntity model)
        {
            return JsonConvert.SerializeObject(model);
        }
    }
}
