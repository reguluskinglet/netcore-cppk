using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Grid;
using Core.Helpers;
using Core.QueryBuilders;
using Dapper;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Rzdppk.Core.Helpers;
using Rzdppk.Model.Base;
using Rzdppk.Core.Grid;
using Rzdppk.Core.Repositoryes.Sqls;

namespace Invent.Core.GridModels.Base
{
    public class ActionGrid<TGridModel, TResult, TFilter>
        where TGridModel : IGridModel<TResult>
        where TResult : BaseEntity
        where TFilter : class
    {
        private readonly IGridOptions _gridOptions;
        private readonly IDbConnection _connection;
        private readonly IFilter _filter;
        private readonly TFilter _optionsFilter;
        private readonly TGridModel _gridModel;
        private readonly string _sql;
        public Action<TResult, JObject> MapResult;
        public Action<TResult, JObject> MapRow;

        public Func<string, TFilter, IEnumerable<TResult>> ResultData;

        public ActionGrid(IDbConnection connection, IGridOptions options, IFilter filter = null)
        {
            _gridOptions = options;
            _connection = connection;
            _filter = filter;
            _optionsFilter = (_gridOptions as IFilterOptions<TFilter>)?.Filter;
            _gridModel = CreateGridModel();
            _sql = _gridModel.GridSql;
        }

        private List<string> _allClientColumns;

        public async Task<object> Render()
        {
            _filter.Apply();

            var pagger = await GetPagger();

            var query = GetSqlGrid(pagger);

            var rows = await _connection.QueryAsync<TResult>(query, _optionsFilter);

            var result = rows.Select(CreateRowResult).ToList();

            return new
            {
                Pager = pagger,
                Rows = result
            };
        }

        public async Task<IEnumerable<TResult>> GetResultRows()
        {
            _filter.Apply();
            var query = GetSqlWithSelectedRows();
            return await _connection.QueryAsync<TResult>(query, _optionsFilter);
        }

        public async Task<List<JObject>> GetChildRows(string sql, int parentId)
        {
            var rows = await _connection.QueryAsync<TResult>(sql, new { parentId= parentId });

            var result = rows.Select(CreateRowResult).ToList();

            return result;
        }

        private string GetSqlGrid(LazyPagination pagger)
        {
            var sql = _sql ?? _gridModel.GridSql;

            sql = _filter.AddCondition(sql);

            var orderColumns = _gridModel.Column.First(x => string.Equals(x.SystemName, _gridOptions.SortOptions.Column, StringComparison.OrdinalIgnoreCase));

            string orderColumn = orderColumns.OrderByName ?? orderColumns.SystemName;

            var orderBy = new OrderByBuilder(orderColumn, _gridOptions.SortOptions.Direction).QueryResult;

            sql += Environment.NewLine + orderBy;            

            sql += Environment.NewLine + pagger.GetOffestRow();

            return sql;
        }

        private async Task<LazyPagination> GetPagger()
        {
            var sql = _gridModel.RowCountSql;

            if (sql == null)
                sql = (_sql ?? _gridModel.GridSql).ReplaceSelect("SELECT count(*)");

            sql = _filter.AddCondition(sql);

            var rowCount = await _connection.QueryFirstAsync<int>(sql, _optionsFilter);

            return new LazyPagination(rowCount, _gridOptions.Page, _gridOptions.PageSize);
        }

        private string GetSqlWithSelectedRows()
        {
            var sql = _sql ?? _gridModel.GridSql;

            if (_gridOptions.SelectedRows.Count > 0)
            {
                if (_gridOptions.IsSelectedAll)
                {
                    List<string> ids = _gridOptions.SelectedRows.Where(x => x.IsSelected == false)
                        .Select(x => x.ParentId == null ? x.RowId.Substring(2) : x.RowId.Replace($"{x.ParentId.Substring(2)}", "").Substring(2)).ToList();

                    sql = _filter.AddCondition(sql, ids, false);
                }
                else
                {
                    List<string> ids = _gridOptions.SelectedRows.Where(x => x.IsSelected == true)
                        .Select(x => x.ParentId == null ? x.RowId.Substring(2) : x.RowId.Replace($"{x.ParentId.Substring(2)}", "").Substring(2)).ToList();

                    sql = _filter.AddCondition(sql, ids, true);
                }                    
            }
            else
            {
                sql = _filter.AddCondition(sql);
            }

            var orderColumns = _gridModel.Column.First(x => string.Equals(x.SystemName, _gridOptions.SortOptions.Column, StringComparison.OrdinalIgnoreCase));

            string orderColumn = orderColumns.OrderByName ?? orderColumns.SystemName;

            var orderBy = new OrderByBuilder(orderColumn, _gridOptions.SortOptions.Direction).QueryResult;

            sql += Environment.NewLine + orderBy;

            return sql;
        }

        private JObject CreateRowResult(TResult row)
        {
            var jobject = new JObject();

            MapRow?.Invoke(row, jobject);

            var array = new JArray(Columns.Select(x => x.GetValue(row)));

            jobject.Add("id", row.Id);
            jobject.Add("item", array);

            MapResult?.Invoke(row, jobject);

            return jobject;
        }

        /// <summary>
        /// Получить Видимые колонки
        /// </summary>
        public IEnumerable<GridColumnInfo> GetVisibleColumns
        {
            get
            {
                return Columns
                    .Where(x => x.DisplayName != null)
                    .Select(column => new GridColumnInfo(column.SystemName, column.DisplayName, column.Width));
            }
        }

        /// <summary>
        /// Получить все колонки
        /// </summary>
        public IEnumerable<GridColumnInfo> GetAllColumns
        {
            get
            {
                return
                    _gridModel.Column.Where(x => x.DisplayName != null).OrderBy(x => x.DisplayName)
                        .Select(column => new GridColumnInfo(column.SystemName, column.DisplayName, column.Width));
            }
        }

        private IEnumerable<GridColumn<TResult>> _columns;

        public IEnumerable<GridColumn<TResult>> Columns
        {
            get
            {
                if (_columns != null)
                    return _columns;

                if (_gridOptions.VisibleColumns.Any())
                {
                    var result = new List<GridColumn<TResult>>();

                    result.AddRange(_gridOptions.VisibleColumns.Select(GetColumn));

                    _columns = result;

                    return _columns;
                }

                _columns = _gridModel.Column.Where(x => x.IsDefault);

                return _columns;
            }

        }

        private GridColumn<TResult> GetColumn(GridColumnInfo visibleColumn)
        {
            var column = _gridModel.Column.First(x =>
                string.Equals(x.SystemName, visibleColumn.Name, StringComparison.OrdinalIgnoreCase));

            if (visibleColumn.Width > 0)
                column.Width = visibleColumn.Width;

            return column;
        }

        /// <summary>
        /// Получить список выбранных записей
        /// </summary>
        public List<GridSelectedRow> GetSelectedRows
        {
            get
            {
                bool isSelectedAll = _gridOptions.IsSelectedAll;

                if (_gridOptions.SelectedRow == null && _gridOptions.IsSelectedAll)
                    _gridOptions.SelectedRows.Clear();

                if (_gridOptions.SelectedRow != null)
                {
                    if (!_gridOptions.SelectedRows.Any(x => x.RowId.Equals(_gridOptions.SelectedRow.RowId)))
                        _gridOptions.SelectedRows.Add(_gridOptions.SelectedRow);
                    else
                        _gridOptions.SelectedRows.Where(x => x.RowId.Equals(_gridOptions.SelectedRow.RowId)).ToList().ForEach(x => x.IsSelected = _gridOptions.SelectedRow.IsSelected);
                }                    

                _gridOptions.SelectedRow = null;

                return _gridOptions.SelectedRows;
            }
        }

        private TGridModel CreateGridModel()
        {
            return Activator.CreateInstance<TGridModel>();
        }
    }
}