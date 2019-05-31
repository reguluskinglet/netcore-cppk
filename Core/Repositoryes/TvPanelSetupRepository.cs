using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model.Enums;
using Rzdppk.Model.TV;

namespace Rzdppk.Core.Repositoryes
{
    public class TvPanelSetupRepository : ITvPanelSetupRepository
    {
        private readonly IDb _db;

        public TvPanelSetupRepository(IDb db)
        {
            _db = db;
        }

        public async Task<DevExtremeTableData.ReportResponse> GetTable(TvPanelRequest input)
        {
            var result = new DevExtremeTableData.ReportResponse
            {
                Rows = new List<DevExtremeTableData.Row>(),
                Columns = new List<DevExtremeTableData.Column>
                {
                    new DevExtremeTableData.Column("col0", "Наименование", "string")
                }
            };


            if (input.ParentId == null)
            {
                //1st level, boxes
                const string sql = @"SELECT * FROM [TvBoxes]";
                var boxes = await _db.Connection.QueryAsync<TVBox>(sql);
                foreach (var box in boxes)
                {
                    result.Rows.Add(new DevExtremeTableData.Row
                    {
                        Id = new DevExtremeTableData.RowId { Id = box.Id, Type = (int)TvPanelLevelEnum.Box },
                        ParentId = null,
                        HasItems = true.ToString(),
                        Col0 = box.Name
                    });
                }
            }
            else
            {
                if (input.ParentId.Type == (int) TvPanelLevelEnum.Box)
                {
                    //query from 1st level - for 2nd level data
                    const string sql = @"select * FROM [TvPanels] p left join [TvBoxes] b on b.Id=p.TVBoxId where b.Id=@Id";

                    var items = await _db.Connection.QueryAsync<TVPanel, TVBox, TVPanel>(sql,
                        (panel, box) =>
                        {
                            panel.TVBox = box;
                            return panel;
                        },
                        new { Id = input.ParentId.Id }
                    );

                    foreach (var groupNum in items.GroupBy(o => o.Number))
                    {
                        var monNum = groupNum.Key;
                        result.Rows.Add(new DevExtremeTableData.Row
                        {
                            Id = new DevExtremeTableData.RowId { Id = input.ParentId.Id, ScreenNum = monNum, Type = (int)TvPanelLevelEnum.Screen },
                            ParentId = input.ParentId,
                            HasItems = true.ToString(),
                            Col0 = $"Экран {monNum}"
                        });
                    }
                }
                else if (input.ParentId.Type == (int)TvPanelLevelEnum.Screen && input.ParentId.ScreenNum.HasValue)
                {
                    const string sql = @"select * FROM [TvPanels] p left join [TvBoxes] b on b.Id=p.TVBoxId where b.Id=@Id and p.Number=@Number";

                    var items = await _db.Connection.QueryAsync<TVPanel, TVBox, TVPanel>(sql,
                        (panel, box) =>
                        {
                            panel.TVBox = box;
                            return panel;
                        },
                        new { Id = input.ParentId.Id, Number = input.ParentId.ScreenNum.Value }
                    );

                    foreach (var item in items)
                    {
                        result.Rows.Add(new DevExtremeTableData.Row
                        {
                            Id = new DevExtremeTableData.RowId { Id = item.Id, Type = (int)TvPanelLevelEnum.Type },
                            ParentId = input.ParentId,
                            HasItems = false.ToString(),
                            Col0 = ScreenTypeEnumToString(item.ScreenType)
                        });
                    }
                }
            }

            result.Rows = DevExtremeTableUtils.DevExtremeTableFiltering(result.Rows, input.Filters);
            result.Rows = DevExtremeTableUtils.DevExtremeTableSorting(result.Rows, input.Sortings);
            result.Total = result.Rows.Count.ToString();
            result.Paging(input.Paging);

            return result;
        }

        public async Task<TvPanelSetupPaging> GetAllBoxes(int skip, int limit)
        {
            const string sql = @"SELECT b.* FROM[TvBoxes] AS b ORDER BY b.Id DESC OFFSET @skip ROWS FETCH NEXT @limit ROWS ONLY";
            const string sqlp = @"SELECT p.* FROM[TvPanels] AS p WHERE p.TVBoxId = @Id ORDER by p.UpdateDate";
            const string sqlc = @"SELECT count(*) FROM [TvBoxes] as b";

            var result = await GetAllTvBoxesWithPanels(sql, sqlp, skip, limit);

            var count = _db.Connection.ExecuteScalar<int>(sqlc);
            var output = new TvPanelSetupPaging
            {
                Data = result,
                Total = count
            };

            return output;
        }

        public async Task<TvPanelSetupPaging> GetAllBoxes(int skip, int limit, string filter)
        {
            string sqlfilter, sql, sqlp, sqlc;

            CreateFilter(filter, out sqlfilter, out sql, out sqlp, out sqlc);

            var result = await GetAllTvBoxesWithPanels(sql, sqlp, skip, limit);
            
            var count = _db.Connection.ExecuteScalar<int>(sqlc);
            var output = new TvPanelSetupPaging
            {
                Data = result,
                Total = count
            };

            return output;
        }

        private async Task<List<TvBoxWithPanels>> GetAllTvBoxesWithPanels(string sql, string sqlp, int skip, int limit)
        {
            var boxWithPanels = new Dictionary<int, TVBox>();

            var tvBoxes = await _db.Connection.QueryAsync<TVBox>(sql, new { skip = skip, limit = limit });
            foreach (var box in tvBoxes)
            {
                TVBox boxEntry = box;

                var panels = await _db.Connection.QueryAsync<TVPanel>(sqlp, new { Id = box.Id });

                if (panels != null)
                    boxEntry.TVPanels = panels;
                else
                    boxEntry.TVPanels = new List<TVPanel>();

                boxWithPanels.Add(box.Id, boxEntry);
            }

            var result = new List<TvBoxWithPanels>();
            foreach (var box in boxWithPanels)
            {
                var panels = new Dictionary<int, List<TvScreenPanel>>();

                if (box.Value.TVPanels.Count() > 0)
                {
                    foreach (var groupNum in box.Value.TVPanels.GroupBy(o => o.Number))
                    {
                        panels[groupNum.Key] = groupNum.Select(o => new TvScreenPanel
                        {
                            Id = o.Id,
                            Type = new ScreenTypeDto
                            {
                                Id = (int)o.ScreenType,
                                Name = ScreenTypeEnumToString(o.ScreenType)
                            }
                        }).ToList();
                    }
                }
                else
                {
                    panels[0] = new List<TvScreenPanel>();
                }

                var item = new TvBoxWithPanels
                {
                    Id = box.Value.Id,
                    Name = box.Value.Name,
                    Panels = panels
                };

                result.Add(item);
            }

            return result.ToList();
        }

        public List<ScreenTypeDto> GetAllScreenTypes()
        {
            var ret = new List<ScreenTypeDto>();

            foreach (ScreenType item in Enum.GetValues(typeof(ScreenType)))
            {
                ret.Add(new ScreenTypeDto
                {
                    Id = (int) item,
                    Name = ScreenTypeEnumToString(item)
                });
            }

            return ret;
        }

        private async Task<TVPanel> GetPanelById(int panelId)
        {
            const string sql = "select * from TvPanels where Id=@Id";

            var panel = await _db.Connection.QuerySingleAsync<TVPanel>(sql, new {Id = panelId});

            return panel;
        }

        private async Task<List<TVPanel>> GetAllBoxPanels(int boxId)
        {
            const string sql = "select * from TvPanels where TvBoxId=@Id";

            var panels = await _db.Connection.QueryAsync<TVPanel>(sql, new {Id = boxId});

            return panels.ToList();
        }

        public async Task DeleteBox(int boxId)
        {
            const string sqlBox = @"DELETE FROM [TvBoxes] WHERE Id = @Id";
            const string sqlPanels = @"DELETE FROM [TvPanels] WHERE TVBoxId = @Id";

            await _db.Connection.ExecuteAsync(sqlBox, new { Id = boxId });
            await _db.Connection.ExecuteAsync(sqlPanels, new { Id = boxId });
        }

        public async Task ChangePanelType(int panelId, ScreenType newType)
        {
            var panel = await GetPanelById(panelId);

            if (panel.ScreenType != newType)
            {
                const string sql = "update TvPanels set ScreenType=@Type where Id=@Id";

                await _db.Connection.ExecuteAsync(sql, new {Type = newType, Id = panelId});
            }
        }

        public async Task<int> AddPanel(PanelAddDto input)
        {
            var allBoxPanels = await GetAllBoxPanels(input.TVBoxId);

            var screenPanelsCount = allBoxPanels.Count(o => o.Number == input.Number);

            if (screenPanelsCount == 0)
            {
                throw new Exception("Добавить экран можно только на существующий монитор");
            }

            const string sql = "insert into TvPanels ([TVBoxId],[Number],[ScreenType]) values(@BoxId,@Num,@Type) SELECT SCOPE_IDENTITY()";

            return await _db.Connection.QueryFirstOrDefaultAsync<int>(sql, new { BoxId = input.TVBoxId, Num = input.Number, Type = input.Type });
        }

        public async Task DeletePanel(int panelId)
        {
            var panel = await GetPanelById(panelId);

            var allBoxPanels = await GetAllBoxPanels(panel.TVBoxId);

            var screenPanelsCount = allBoxPanels.Count(o => o.Number == panel.Number);
            if (screenPanelsCount == 1)
            {
                //change type to none/empty
                await ChangePanelType(panelId, ScreenType.None);
            } else if (screenPanelsCount > 1)
            {
                //delete
                var sql = "delete from TvPanels where Id=@Id";
                await _db.Connection.ExecuteAsync(sql, new {Id = panel.Id});
            }
        }

        private static string ScreenTypeEnumToString(ScreenType type)
        {
            var ret = $"неизвестный тип ({type.ToString()})";

            switch (type)
            {
                case ScreenType.None:
                    ret = "Начальный экран";
                    break;
                case ScreenType.ScheduleDeviationTable:
                    ret = "Расхождение фактического и планового графика рейсов на маршрутах";
                    break;
                case ScreenType.ScheduleDeviationGraph:
                    ret = "Расхождение фактического и планового графика (диаграмма)";
                    break;
                case ScreenType.BrigadeScheduleDeviationTable:
                    ret = "Расхождение фактического и планового графика для сотрудников бригад";
                    break;
                case ScreenType.ToDeviationTable:
                    ret = "Расхождение фактического и планового графика для плановых мероприятий ";
                    break;
                case ScreenType.CriticalMalfunctionsTable:
                    ret = "Перечень составов с неисправностями";
                    break;
                case ScreenType.TrainsInDepoMalfunctionsTable:
                    ret = "Количество неисправностей на составах, запланированных для ремонтных работ и ТО";
                    break;
                case ScreenType.TrainsInDepoStatusTable:
                    ret = "Состояние составов в депо";
                    break;
                case ScreenType.Journals:
                    ret = "События";
                    break;
            }

            return ret;
        }

        private static void CreateFilter(string filter, out string sqlfilter, out string sql, out string sqlp, out string sqlc)
        {
            var filters = JsonConvert.DeserializeObject<Other.Other.FilterBody[]>(filter);

            sqlfilter = "WHERE ";
            for (var index = 0; index < filters.Length; index++)
            {
                var item = filters[index];
                sqlfilter = $"{sqlfilter} b.[{item.Filter}] LIKE '%{item.Value}%' ";
                if (index < (filters.Length - 1))
                    sqlfilter = $"{sqlfilter} AND ";

            }

            sql = $"SELECT b.* FROM[TvBoxes] AS b {sqlfilter} ORDER BY b.Id DESC OFFSET @skip ROWS FETCH NEXT @limit ROWS ONLY";
            sqlp = $"SELECT p.* FROM[TvPanels] AS p WHERE p.TVBoxId = @Id ORDER by p.UpdateDate";
            sqlc = $"SELECT count(*) FROM [TvBoxes] as b {sqlfilter}";
        }
    }

    public enum TvPanelLevelEnum
    {
        Box = 1,
        Screen = 2,
        Type = 3
    }

    public class TvBoxWithPanels
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Dictionary<int, List<TvScreenPanel>> Panels { get; set; } //idx = screen num
    }

    public class TvScreenPanel
    {
        public int Id { get; set; }

        //public int Number { get; set; }

        public ScreenTypeDto Type { get; set; }
    }

    public class ScreenTypeDto
    {
        public int Id {get; set; }

        public string Name { get; set; }
    }

    public class PanelTypeUpdateDto
    {
        public int Id { get; set; }

        public ScreenType Type { get; set; }
    }

    public class PanelAddDto
    {
        public int TVBoxId { get; set; }

        public int Number { get; set; }

        public ScreenType Type { get; set; }
    }

    public class TvPanelRequest
    {
        public DevExtremeTableData.RowId ParentId { get; set; }
        public DevExtremeTableData.Paging Paging { get; set; }
        public List<DevExtremeTableData.Filter> Filters { get; set; }
        public List<DevExtremeTableData.Sorting> Sortings { get; set; }
    }

    public class TvPanelSetupPaging
    {
        public List<TvBoxWithPanels> Data { get; set; }
        public int Total { get; set; }
    }
}