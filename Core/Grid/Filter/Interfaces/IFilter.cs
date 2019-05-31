using System;
using System.Collections.Generic;
using System.Dynamic;
using Core.QueryBuilders;

namespace Core.Grid
{
    public interface IFilter
    {
        List<string> Condition { get; }
        void Init(object options);
        void Apply();
        object FilterOption { get; }
        string DefaultCondition { get; }
        string AddCondition(string currentSql);
        string AddCondition(string currentSql, List<string> ids, bool inCondition);
    }
}