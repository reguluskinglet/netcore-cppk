using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Grid.Enums;
using Dapper;
using Invent.Core.GridModels.Base;
using Microsoft.AspNetCore.Mvc;
using Rzdppk.Core.GridModels.Dispatcher;
using Rzdppk.Core.GridModels.Goep;
using Rzdppk.Core.GridModels.Journals;
using Rzdppk.Core.GridModels.Route;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Services.Interfaces;

namespace Rzdppk.Controllers
{
    [Route("api/[controller]/[action]")]
    public class GridController : Controller
    {
        private readonly IDb _db;
        private readonly ICommonService _commonService;

        public GridController
            (
                IDb db,
                ICommonService commonService
            )
        {
            _db = db;
            _commonService = commonService;
        }

        public async Task<IActionResult> Journal(JournalGridOptions options, CommandType command)
        {
            if (command == CommandType.Save)
                return Ok();

            if (command == CommandType.Clear)
            {
                options.Page = 1;
                options.Filter = new JournalFilter();
                options.SetSortOptionsDefault();
                options.SetSelectedRowsDefault();
            }

            var filter = new JournalGridFilter();

            filter.Init(options.Filter);

            if(filter.IsEmptyFilter())
                options.SetSelectedRowsDefault();

            //var user = await _authService.GetCurrentUser();

            //filter.ApplyPermission(user);

            if (command == CommandType.None || command == CommandType.Clear)
                filter.Configure();

            var grid = new ActionGrid<JournalGrid, JournalGridModel, JournalFilter>(_db.Connection, options, filter)
            {
                MapResult = (row, result) =>
                {
                    var rowId = row.HasInspection ? "M_" + row.Id : "E_" + row.Id;
                    result["id"] = rowId;
                    result.Add("parentId", null);
                    result.Add("childCount", row.Count);
                    result.Add("hasInspection", row.HasInspection);
                    //result.Add("isSelected", options.IsSelectedRow(rowId));
                }
            };

            filter.IsEmptyFilter();

            var data = await grid.Render();

            if (command == CommandType.Page)
                return Ok(new
                {
                    isEmptyFilter = filter.IsEmptyFilter(),
                    DataSource = data,
                });

            return Ok(new
            {
                FilterInfo = filter.GetVisibleFilters(options.VisibleFilters),
                Filter = options.Filter,
                isEmptyFilter = filter.IsEmptyFilter(),
                SortOptions = options.SortOptions,
                AllColumns = grid.GetAllColumns,
                VisibleColumns = grid.GetVisibleColumns,
                DataSource = data,
                IsSelectedAll = options.IsSelectedAll,
                SelectedRows = grid.GetSelectedRows,
                Data = new
                {
                    DataSource = await _commonService.GetAllReference()
                }
            });
        }

        public async Task<IActionResult> JournalGetChild(JournalGridOptions options, [FromBody]int parentId)
        {
            var grid = new ActionGrid<JournalGrid, JournalGridModel, JournalGridFilter>(_db.Connection, options)
            {
                MapResult = (row, result) =>
                {
                    result["id"] = parentId+ "E_" + row.Id;
                    result.Add("parentId", "M_" + parentId);
                    result.Add("childCount", row.Count);
                    result.Add("hasInspection", row.HasInspection);
                }
            };

            return Ok(await grid.GetChildRows(Sql.SqlQueryCach["Journal.GridGetChildByParentId"], parentId));
        }

        public async Task<IActionResult> Goep(GoepGridOptions options, CommandType command)
        {
            if (command == CommandType.Save)
                return Ok();

            if (command == CommandType.Clear)
            {
                options.Page = 1;
                options.Filter = new GoepFilter();
                options.SetSortOptionsDefault();
            }

            var filter = new GoepGridFilter();

            filter.Init(options.Filter);

            //var user = await _authService.GetCurrentUser();

            //filter.ApplyPermission(user);

            if (command == CommandType.None || command == CommandType.Clear)
                filter.Configure();

            var grid = new ActionGrid<GoepGrid, GoepGridModel, GoepFilter>(_db.Connection, options, filter)
            {
                MapRow = (row, result) =>
                {
                    row.Days=_db.Connection.Query<int>(Sql.SqlQueryCach["Turnovers.GetDaysByTurnoverId"],new{id=row.Id}).ToList();
                }
            };

            var data = await grid.Render();

            if (command == CommandType.Page)
                return Ok(new
                {
                    isEmptyFilter = filter.IsEmptyFilter(),
                    DataSource = data,
                });

            return Ok(new
            {
                FilterInfo = filter.GetVisibleFilters(options.VisibleFilters),
                Filter = options.Filter,
                isEmptyFilter = filter.IsEmptyFilter(),
                SortOptions = options.SortOptions,
                AllColumns = grid.GetAllColumns,
                VisibleColumns = grid.GetVisibleColumns,
                DataSource = data,
                IsSelectedAll = options.IsSelectedAll,
                SelectedRows = grid.GetSelectedRows
            });
        }

        public async Task<IActionResult> Route(RouteGridOptions options, CommandType command)
        {
            if (command == CommandType.Save)
                return Ok();

            if (command == CommandType.Clear)
            {
                options.Page = 1;
                options.Filter = new RouteFilter();
                options.SetSortOptionsDefault();
            }

            var filter = new RouteGridFilter();

            filter.Init(options.Filter);

            //var user = await _authService.GetCurrentUser();

            //filter.ApplyPermission(user);

            if (command == CommandType.None || command == CommandType.Clear)
                filter.Configure();

            var grid = new ActionGrid<RouteGrid, RouteGridModel, RouteFilter>(_db.Connection, options, filter)
            {
                MapRow = (row, result) =>
                {
                    row.Days = _db.Connection.Query<int>(Sql.SqlQueryCach["Trips.Days"], new { id = row.Id }).ToList();
                    row.Stantions= _db.Connection.Query<TripStantion>(Sql.SqlQueryCach["Trips.Stantion"], new { id = row.Id }).ToList();

                }
            };

            var data = await grid.Render();

            if (command == CommandType.Page)
                return Ok(new
                {
                    isEmptyFilter = filter.IsEmptyFilter(),
                    DataSource = data,
                });

            return Ok(new
            {
                FilterInfo = filter.GetVisibleFilters(options.VisibleFilters),
                Filter = options.Filter,
                isEmptyFilter = filter.IsEmptyFilter(),
                SortOptions = options.SortOptions,
                AllColumns = grid.GetAllColumns,
                VisibleColumns = grid.GetVisibleColumns,
                DataSource = data,
                IsSelectedAll = options.IsSelectedAll,
                SelectedRows = grid.GetSelectedRows
            });
        }


        public async Task<IActionResult> Dispatcher(DispatcherGridOptions options, CommandType command)
        {
            if (command == CommandType.Save)
                return Ok();

            if (command == CommandType.Clear)
            {
                options.Page = 1;
                options.Filter = new DispatcherFilter();
                options.SetSortOptionsDefault();
            }

            var filter = new DispatcherGridFilter();

            filter.Init(options.Filter);

            //var user = await _authService.GetCurrentUser();

            //filter.ApplyPermission(user);

            if (command == CommandType.None || command == CommandType.Clear)
                filter.Configure();

            var grid = new ActionGrid<DispatcherGrid, DispatcherGridModel, DispatcherFilter>(_db.Connection, options, filter);

            var data = await grid.Render();

            if (command == CommandType.Page)
                return Ok(new
                {
                    isEmptyFilter = filter.IsEmptyFilter(),
                    DataSource = data,
                });

            return Ok(new
            {
                FilterInfo = filter.GetVisibleFilters(options.VisibleFilters),
                Filter = options.Filter,
                isEmptyFilter = false,
                SortOptions = options.SortOptions,
                AllColumns = grid.GetAllColumns,
                VisibleColumns = grid.GetVisibleColumns,
                DataSource = data,
                IsSelectedAll = options.IsSelectedAll,
                SelectedRows = grid.GetSelectedRows,
                Data = new
                {
                    DataSource = await _commonService.GetDepoEventDataSource()
                }
            });
        }
    }
}