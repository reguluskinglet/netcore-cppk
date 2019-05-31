using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rzdppk.Core.Repositoryes.Sqls
{
    public static class Sql
    {
        public static Dictionary<string, string> SqlQueryCach;

        static Sql()
        {
            SqlQueryCach = new Dictionary<string, string>();
            Initialize();
        }


        #region Private

        private static void Initialize()
        {
            var assembly = typeof(Sql).Assembly;

            var resourceNames = assembly.GetManifestResourceNames();

            foreach (var resourceName in resourceNames)
            {
                if(resourceName.Contains(".xlsx"))
                    continue;

                var stream = assembly.GetManifestResourceStream(resourceName);

                if(stream == null)
                    throw new Exception($"Не удалось найти файл SQL {resourceName}");

                var splits = resourceName.Split('.');

                var name = $"{splits[splits.Length - 3]}.{splits[splits.Length - 2]}";

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var sql = reader.ReadToEnd();

                    SqlQueryCach.Add(name, sql);
                }
            }
        }

        #endregion

    }
}
