using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Grid;
using Core.Helpers;
using Core.QueryBuilders;
using Dapper;
using Newtonsoft.Json.Linq;
using Rzdppk.Core.Helpers;
using Rzdppk.Model.Base;

namespace Invent.Core.GridModels.Base
{
    public class RowGrid<TGridModel,TResult>
        where TGridModel : IGridModel<TResult>
        where TResult :  BaseEntity
    {
        private readonly IGridOptions _gridOptions;
        private readonly TGridModel _gridModel;
        private TResult _model;

        public RowGrid(TResult model, IGridOptions options)
        {
            _model = model;
            _gridOptions = options;
            _gridModel = CreateGridModel();
        }

        public JObject GetRow()
        {
            if(_model is IHierarchicalModel<TResult> )
                return CreateRowResultHierarchical(_model);

            return CreateRowResult(_model);
        }

        private JObject CreateRowResult(TResult row)
        {
            var jobject = new JObject();

            var array = new JArray(Columns.Select(x => x.GetValue(row)));

            jobject.Add("id", row.Id);
            jobject.Add("item", array);

            return jobject;
        }

        private int _maxLevel = 0;

        private JObject CreateRowResultHierarchical(TResult row)
        {
            var hierarchicalRow =(IHierarchicalModel<TResult>)row;

            var jobject = new JObject();

            if (hierarchicalRow.Level > _maxLevel)
                _maxLevel = hierarchicalRow.Level;

            var array = new JArray(Columns.Select(x => x.GetValue(row)));

            jobject.Add("id", hierarchicalRow.Id);
            jobject.Add("level", hierarchicalRow.Level);
            jobject.Add("item", array);

            if (hierarchicalRow.Child != null)
                jobject.Add("child", new JArray(hierarchicalRow.Child.Select(CreateRowResult)));

            return jobject;
        }

        private IEnumerable<GridColumn<TResult>> _columns;

        private IEnumerable<GridColumn<TResult>> Columns
        {
            get
            {
                if (_columns != null)
                    return _columns;

                if (_gridOptions.VisibleColumns.Any())
                {
                    var result = new List<GridColumn<TResult>>();

                    foreach (var visibleColumn in _gridOptions.VisibleColumns)
                    {
                        var column = _gridModel.Column.First(x => string.Equals(x.SystemName, visibleColumn.Name, StringComparison.OrdinalIgnoreCase));

                        if (visibleColumn.Width > 0)
                            column.Width = visibleColumn.Width;

                        result.Add(column);
                    }

                    _columns = result;

                    return _columns;
                }

                _columns = _gridModel.Column.Where(x => x.IsDefault);
                return _columns;
            }

        }

        private TGridModel CreateGridModel()
        {
            return Activator.CreateInstance<TGridModel>();
        }
    }
}
