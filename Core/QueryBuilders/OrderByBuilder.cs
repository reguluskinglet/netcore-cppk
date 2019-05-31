using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.QueryBuilders
{
    public struct OrderByBuilder
    {
        private readonly string _fieldOrder;
        private readonly SortDirection _sortDirection;

        public OrderByBuilder(string orderField, SortDirection sortDirection)
        {
            _fieldOrder = orderField;
            _sortDirection = sortDirection;
        }

        public string QueryResult => $"ORDER BY {_fieldOrder} "+ _sortDirection;
    }
}
