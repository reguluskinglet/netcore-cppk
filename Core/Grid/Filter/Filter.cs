using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Core.Grid.Filter;
using Core.Helpers;
using Core.QueryBuilders;
using Rzdppk.Core.Helpers;

namespace Core.Grid
{
    public class Filter<TFilter> : IFilter
         where TFilter : class 
    {
        private readonly List<string> _condition;

        private List<FilterInfo> _filtres;
        private TFilter _filterOptions;
        public string DefaultCondition { get; private set; }

        public Filter(string defaultCondition=null)
        {
            DefaultCondition = defaultCondition;
            _condition = new List<string>();
            _filtres = new List<FilterInfo>();
        }

        protected virtual WhereBuilder? GetWhere()
        {
            WhereBuilder? condition = null;

            if (Condition.Any())
                condition = new WhereBuilder(Condition);                

            return condition;
        }

        public virtual string AddCondition(string currentSql)
        {
            var condition = GetWhere();

            if (condition.HasValue)
                return currentSql.ReplaceAnd(condition.Value.QueryAndResult);

            return currentSql;
        }

        public virtual string AddCondition(string currentSql, List<string> ids, bool inCondition = true)
        {
            var condition = GetWhere();

            if (condition.HasValue)
                return currentSql.ReplaceAnd(condition.Value.QueryAndResult);

            return currentSql;
        }

        public virtual void Apply()
        {
        }

        public List<string> Condition
        {
            get { return _condition; }
        }

        public object FilterOption => _filterOptions;

        public void Init(object options) 
        {
            _filterOptions = (TFilter)options;
        }

        public virtual Filter<TFilter> Configure()
        {
            return new Filter<TFilter>();
        }

        public virtual bool IsEmptyFilter()
        {
            Type type = FilterOption.GetType();

            IList<PropertyInfo> props = new List<PropertyInfo>(type.GetProperties());

            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(FilterOption, null) != null)
                    return false;
            }

            return true;
        }

        public List<FilterInfo> GetVisibleFilters(List<string> visibleFiltres = null)
        {
            return visibleFiltres!=null && visibleFiltres.Any() 
                ? _filtres.Where(x => visibleFiltres.Contains(x.ConditionName, StringComparer.OrdinalIgnoreCase)).ToList() 
                : _filtres;
        }

        public void AddTextFilter(Expression<Func<TFilter, object>> propertySpecifier)
        {
            AddFilter(propertySpecifier, FilterType.Text);
        }

        public void AddAutocompliteFilter(Expression<Func<TFilter, object>> propertySpecifier, dynamic data)
        {
            AddFilter(propertySpecifier, FilterType.Autocomplite, data);
        }

        public void AddSelectFilter(Expression<Func<TFilter, object>> propertySpecifier, dynamic data=null)
        {
            AddFilter(propertySpecifier, FilterType.Select, data);
        }

        public void AddSelectFilter(Expression<Func<TFilter, object>> propertySpecifier, Func<object,object> filterData)
        {
            var value = propertySpecifier.Compile()(_filterOptions);

            var data = filterData(value);

            AddFilter(propertySpecifier, FilterType.Autocomplite, data);
        }


        public void AddFilter(Expression<Func<TFilter, object>> propertySpecifier, FilterType type,  dynamic data=null)
        {
            var expression = propertySpecifier.Body as UnaryExpression;

            var name = ((MemberExpression) (expression?.Operand ?? propertySpecifier.Body)).Member.Name;

            _filtres.Add(new FilterInfo
            {
                ConditionName = name.FirstCharacterToLower(),
                Data = data,
                FilterType = type
            });
        }

        public void AddDateFilter(string conditionName)
        {
            _filtres.Add(new FilterInfo
            {
                ConditionName = conditionName.FirstCharacterToLower(),
                FilterType = FilterType.Date
            });
        }

        public void AddCondition(Expression<Func<TFilter, object>> propertySpecifier, ConditionType conditionType, string sqlProperty=null, object defaultValue = null)
        {
            var value = defaultValue ?? propertySpecifier.Compile()(_filterOptions);

            if (value == null)
                return;

            string name = GetMemberName(propertySpecifier);

            sqlProperty = sqlProperty ?? $"{propertySpecifier.Parameters[0].Name}.{name}";

            switch (conditionType)
            {
                case ConditionType.Eq:
                    Eq(name, sqlProperty);
                    break;
                case ConditionType.LikeStart:
                case ConditionType.LikeAny:
                    Like(name, sqlProperty, conditionType);
                    break;
                case ConditionType.In:
                    var ids= (List<int>)value;

                    if (ids.Any())
                        In(name, sqlProperty);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void AddCondition(Expression<Func<TFilter, object>> propertySpecifier, Action<object> action)
        {
            var value = propertySpecifier.Compile()(_filterOptions);

            if (value == null)
                return;

            action(value);
        }

        public void AddDateFilter(Expression<Func<TFilter, object>> startDate, Expression<Func<TFilter, object>> endDate, string name=null)
        {
            AddCondition(startDate, endDate, (start, end) =>
            {
                string nameFrom = GetMemberName(startDate);
                string nameTo = GetMemberName(endDate);

                var sqlName = name ?? nameFrom.Replace("Start", "");

                DateTime? dateFrom = (DateTime?)start;
                DateTime? dateTo = (DateTime?)end;

                if(dateFrom.HasValue)
                    _condition.Add($"{sqlName} >= @{nameFrom}");

                if (dateTo.HasValue)
                {
                    dateTo = new DateTime(dateTo.Value.Year, dateTo.Value.Month, dateTo.Value.Day, 23, 59, 59);

                    _filterOptions.GetType().GetProperty(nameTo).SetValue(_filterOptions, dateTo);

                    _condition.Add($"{sqlName} <= @{nameTo}");
                }
            });
        }

        public void AddCondition(Expression<Func<TFilter, object>> propertySpecifier, Expression<Func<TFilter, object>> propertySpecifierEnd, Action<object,object> action)
        {
            var value = propertySpecifier.Compile()(_filterOptions);
            var valueEnd = propertySpecifierEnd.Compile()(_filterOptions);

            if (value == null && valueEnd == null)
                return;

            action(value, valueEnd);
        }

        private void Eq(string filterName, string sqlProperty)
        {
            _condition.Add($"{sqlProperty} = @{filterName}");
        }

        private void In(string filterName, string sqlProperty)
        {
            _condition.Add($"{sqlProperty} IN @{filterName}");
        }

        private void Like(string filterName, string sqlProperty, ConditionType type)
        {
            string predicate;

            switch (type)
            {
                case ConditionType.LikeAny:
                    predicate = $"'%'+@{filterName}+'%'";
                    break;
                case ConditionType.LikeStart:
                    predicate = $"@{filterName}+'%'";
                    break;
                default:
                    throw new Exception("ConditionType для конструкции Like не определен");
            }

            _condition.Add($"{sqlProperty} LIKE {predicate}");
        }

        private string GetMemberName(Expression<Func<TFilter, object>> propertySpecifier)
        {
            var body = propertySpecifier.Body as UnaryExpression;

            var expression = (MemberExpression)(body?.Operand ?? propertySpecifier.Body);

            string name = expression.Member.Name;

            return name;
        }
    }
}
