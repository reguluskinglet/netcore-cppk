using System.Collections.Generic;

namespace Core.QueryBuilders
{
    public struct WhereBuilder
    {
        private readonly List<string> _whereAnd;
        private readonly List<string> _whereOr;

        public WhereBuilder(string where)
        {
            _whereAnd = new List<string>
            {
                where
            };

            _whereOr = new List<string> {};
        }

        public WhereBuilder(List<string> where)
        {
            _whereAnd = where;
            _whereOr = new List<string> { };
        }

        public WhereBuilder And(string where)
        {
            _whereAnd.Add(where);

            return this;
        }

        public WhereBuilder Or(string where)
        {
            _whereOr.Add(where);

            return this;
        }

        public List<string> GetWhereAnd => _whereAnd;

        public string QueryResult => _whereAnd.Count > 0 ? $"WHERE {string.Join(" and ", _whereAnd)}":"";

        public string QueryAndResult => _whereAnd.Count > 0 ? $"and {string.Join(" and ", _whereAnd)}" : "";

        public string QueryOrResult => _whereAnd.Count > 0 ? $"or {string.Join(" and ", _whereAnd)}" : "";
    }
}
