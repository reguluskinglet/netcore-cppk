using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rzdppk.Core.Helpers
{
    public static class SqlPattern
    {
        private static readonly Regex TableNamePattern = new Regex(@"--TableName.*");
        private static readonly Regex OrderByPattern = new Regex(@"--ORDER BY.*");
        private static readonly Regex SelectPattern = new Regex(@"^SELECT.*");
        private static readonly Regex UnionAllPattern = new Regex(@"UNION ALL.*");

        private static readonly Regex InPattern = new Regex(@"--IN.*");

        private static readonly Regex OrPattern = new Regex(@"--or.*");
        private static readonly Regex OrBodyPattern = new Regex(@"or.*");
        private static readonly Regex AndPattern = new Regex(@"--and.*");
        private static readonly Regex SubPattern = new Regex(@"--sub.*");

        private static readonly Regex WherePattern = new Regex(@"--WHERE.*");

        private static readonly Regex WhereExPattern = new Regex(@"^WHERE.*", RegexOptions.Multiline);
        private static readonly Regex WhereStartPattern = new Regex(@"^WHERE", RegexOptions.Multiline);

        private const string SetPattern = "--SET";
        private const string WhereIds = "WHERE Id IN @ids";

        public static string ReplaceTableName(this string sql, string tableName)
        {
            return TableNamePattern.Replace(sql, tableName);
        }

        public static string ReplaceOrderByName(this string sql, string order)
        {
            return OrderByPattern.Replace(sql, order);
        }

        public static string ReplaceSelect(this string sql, string order)
        {
            return SelectPattern.Replace(sql, order);
        }

        public static string ReplaceUnionAll(this string sql, string replacment)
        {
            return UnionAllPattern.Replace(sql, replacment);
        }

        public static string ReplaceIn(this string sql, string replacment)
        {
            return InPattern.Replace(sql, replacment);
        }

        public static string ReplaceOr(this string sql, string replacment)
        {
            return OrPattern.Replace(sql, replacment);
        }

        public static string ReplaceOrBody(this string sql, string replacment)
        {
            return OrBodyPattern.Replace(sql, replacment);
        }

        public static string ReplaceAnd(this string sql, string replacment)
        {
            return AndPattern.Replace(sql, replacment);
        }

        public static string ReplaceSub(this string sql, string replacment)
        {
            return SubPattern.Replace(sql, replacment);
        }

        public static string ReplaceWhere(this string sql, string replacment)
        {
            return WherePattern.Replace(sql, replacment);
        }

        public static string ReplaceWhereEx(this string sql, string replacment)
        {
            return WhereExPattern.Replace(sql, replacment);
        }

        public static string ReplaceWhereStart(this string sql)
        {
            return WhereStartPattern.Replace(sql, "");
        }

        public static string ReplaceSet(this string sql, string replacment)
        {
            return sql.Replace(SetPattern, replacment);
        }

        public static StringBuilder ReplaceSet(this StringBuilder sql, string replacment)
        {
            sql.Replace(SetPattern, replacment);

            return sql;
        }

        public static string ReplaceWhereIdToIds(this string sql)
        {
            return WhereExPattern.Replace(sql, WhereIds);
        }
    }
}
