using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using NLog;
using static Rzdppk.Core.Other.DevExtremeTableData;

namespace ConsoleTester
{
    class Program
    {
        static void Main(string[] args)
        {
            

            Console.ReadLine();
        }
    }
    //    private static List<Row> DevExtremeTableSorting(List<Row> results, List<Sorting> sortings)
    //    {
    //        foreach (var sorting in sortings)
    //        {
    //            IOrderedQueryable<Row> sortResults;
    //            switch (sorting.Direction)
    //            {
    //                case "ASC":
    //                    sortResults = results.AsQueryable().OrderBy(sorting.ColumnName);
    //                    results = sortResults.ToList();
    //                    break;
    //                case "DESC":
    //                    sortResults = results.AsQueryable().OrderByDescending(sorting.ColumnName);
    //                    results = sortResults.ToList();
    //                    break;
    //            }

    //        }

    //        return results;
    //    }

    //    private static void DevExtremeTableFiltering(List<Row> testList, List<Row> results, List<Filter> filters)
    //    {
    //        foreach (var filter in filters)
    //        {


    //            foreach (var test in testList)
    //            {
    //                var propertyValue = GetPropValue(test, filter.ColumnName);
    //                //datetime
    //                if (DateTime.TryParse(propertyValue.ToString(), out var timeValue))
    //                {
    //                    if (!DateTime.TryParse(filter.Value, out var filterTimeValue))
    //                        throw new ValidationException("Значение фильтра не DateTime");
    //                    DateTimeFilter(results, filter, test, timeValue, filterTimeValue);
    //                    continue;

    //                }

    //                //int
    //                if (int.TryParse(propertyValue.ToString(), out var intValue))
    //                {

    //                    if (!int.TryParse(filter.Value, out var filterIntValue))
    //                        throw new ValidationException("Значение фильтра не int");
    //                    IntFilter(results, filter, test, intValue, filterIntValue);
    //                    continue;
    //                }

    //                //string
    //                StringFilter(results, filter, test, propertyValue);

    //            }
    //        }
    //    }

    //    private static void StringFilter(List<Row> results, Filter filter, Row test, object propertyValue)
    //    {
    //        switch (filter.Operation)
    //        {
    //            case "contains":
    //                if (propertyValue.ToString().Contains(filter.Value))
    //                    results.Add(test);
    //                break;
    //            case "notContains":
    //                if (!propertyValue.ToString().Contains(filter.Value))
    //                    results.Add(test);
    //                break;
    //        }
    //    }

    //    private static void IntFilter(List<Row> results, Filter filter, Row test, int intValue, int filterIntValue)
    //    {
    //        switch (filter.Operation)
    //        {
    //            case "greaterThan":
    //                if (intValue > filterIntValue)
    //                    results.Add(test);
    //                break;
    //            case "lessThan":
    //                if (intValue < filterIntValue)
    //                    results.Add(test);
    //                break;
    //            case "graterThenOrEqual":
    //                if (intValue >= filterIntValue)
    //                    results.Add(test);
    //                break;
    //            case "lessThanOrEqual":
    //                if (intValue <= filterIntValue)
    //                    results.Add(test);
    //                break;
    //        }
    //    }

    //    private static void DateTimeFilter(List<Row> results, Filter filter, Row test, DateTime timeValue, DateTime filterTimeValue)
    //    {
    //        switch (filter.Operation)
    //        {
    //            case "greaterThan":
    //                if (timeValue > filterTimeValue)
    //                    results.Add(test);
    //                break;
    //            case "lessThan":
    //                if (timeValue < filterTimeValue)
    //                    results.Add(test);
    //                break;
    //            case "graterThenOrEqual":
    //                if (timeValue >= filterTimeValue)
    //                    results.Add(test);
    //                break;
    //            case "lessThanOrEqual":
    //                if (timeValue <= filterTimeValue)
    //                    results.Add(test);
    //                break;
    //        }
    //    }

    //    private void Gavno<T>()
    //    {
    //        var proper = typeof(T).GetProperties();
    //        Console.ReadLine();
    //    }



    //    public static object GetPropValue(object source, string propertyName)
    //    {
    //        var property = source.GetType().GetRuntimeProperties().FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
    //        if (property != null)
    //        {
    //            var value = property.GetValue(source);
    //            return value;
    //        }
    //        return null;
    //    }



    //}


    //public static class QueryableExtensions
    //{
    //    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
    //    {
    //        return source.OrderBy(ToLambda<T>(propertyName));
    //    }

    //    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
    //    {
    //        return source.OrderByDescending(ToLambda<T>(propertyName));
    //    }

    //    private static Expression<Func<T, object>> ToLambda<T>(string propertyName)
    //    {
    //        var parameter = Expression.Parameter(typeof(T));
    //        var property = Expression.Property(parameter, propertyName);
    //        var propAsObject = Expression.Convert(property, typeof(object));

    //        return Expression.Lambda<Func<T, object>>(propAsObject, parameter);
    //    }
    //}



}
