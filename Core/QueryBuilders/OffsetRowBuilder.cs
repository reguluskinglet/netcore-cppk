using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.QueryBuilders
{
    public struct OffsetRowBuilder
    {
        private readonly int _scip;
        private readonly int _take;

        public OffsetRowBuilder(int scip, int take)
        {
            _scip = scip;
            _take = take;
        }

        internal string QueryResult => $"OFFSET ({_scip}) ROWS FETCH FIRST ({_take}) ROWS ONLY";
    }
}
