using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Grid
{
    public struct ColumnJoin
    {
        public string TableName { get; set; }

        public string ForigenKey { get; set; }

        public string Property { get; set; }

        public ColumnJoin(string tableName, string forigenKey, string property = "Name")
        {
            TableName = tableName;
            ForigenKey = forigenKey;
            Property = property;
        }

        public string JoinQuery => $"LEFT JOIN {TableName} ON {TableName}.Id = {ForigenKey}";

        public string OrderName => $"{TableName}.{Property}";
    }
}
