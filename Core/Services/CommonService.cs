using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Sqls;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Core.ViewModels;

namespace Rzdppk.Core.Services
{
    public class CommonService : ICommonService
    {
        private readonly IDb _db;

        public CommonService
            (
                IDb db
            )
        {
            _db = db;
        }

        public async Task<AutocompliteData> GetAllReference()
        {
            return new AutocompliteData
            {
                Users = await _db.Connection.QueryAsync<SelectItem>(Sql.SqlQueryCach["Users.SelectItem"]),
                Stantions = await _db.Connection.QueryAsync<SelectItem>(Sql.SqlQueryCach["Stantion.SelectItemDepo"]),
                Trains = await _db.Connection.QueryAsync<SelectItemDependent>(Sql.SqlQueryCach["Train.SelectItem"]),
                Carriages = await _db.Connection.QueryAsync<SelectItemDependent>(Sql.SqlQueryCach["Carriage.SelectItem"]),
                Equipments = await _db.Connection.QueryAsync<SelectItem>(Sql.SqlQueryCach["Equipment.SelectItem"]),
                
            };
        }

        public async Task<TripSource> GetTripSource()
        {
            return new TripSource
            {
                Stantions = await _db.Connection.QueryAsync<SelectItem>(Sql.SqlQueryCach["Stantion.SelectItem"]),
            };
        }

        public Task<IEnumerable<SelectItem>> GetDirectionsSelectItem()
        {
            return _db.Connection.QueryAsync<SelectItem>(Sql.SqlQueryCach["Turnovers.DirectionsSelectItem"]);
        }

        public async Task<DepoEventDataSource> GetDepoEventDataSource()
        {
            var now = DateTime.Now;
            var dateFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0, now.Kind);
            var dateTo = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999, now.Kind);

            return new DepoEventDataSource
            {
                Users = await _db.Connection.QueryAsync<SelectItem>(Sql.SqlQueryCach["Users.SelectItem"]),
                Trains = await _db.Connection.QueryAsync<SelectItemDependent>(Sql.SqlQueryCach["Train.SelectItem"]),
                Routes = await _db.Connection.QueryAsync<SelectItemDepoEvent>(Sql.SqlQueryCach["Routes.SelectItemToDay"],new
                {
                    DateFrom= dateFrom,
                    DateTo= dateTo
                }),
                Stantions = await _db.Connection.QueryAsync<SelectItem>(Sql.SqlQueryCach["Stantion.SelectItemDepo"]),
                Parkings = await _db.Connection.QueryAsync<SelectItemDependent>(Sql.SqlQueryCach["Parkings.SelectItem"]),
            };
        }
    }

    public class DepoEventDataSource
    {
        public IEnumerable<SelectItem> Users { get; set; }
        public IEnumerable<SelectItemDependent> Trains { get; set; }
        public IEnumerable<SelectItem> Routes { get; set; }
        public IEnumerable<SelectItem> Stantions { get; set; }
        public IEnumerable<SelectItemDependent> Parkings { get; set; }
        public IEnumerable<SelectItem> Inspections { get; set; }
    }

    public class TripSource
    {
        public IEnumerable<SelectItem> Stantions { get; set; }
    }

    public class AutocompliteData
    {
        public IEnumerable<SelectItem> Users { get; set; }
        public IEnumerable<SelectItem> Stantions { get; set; }
        public IEnumerable<SelectItemDependent> Trains { get; set; }
        public IEnumerable<SelectItemDependent> Carriages { get; set; }
        public IEnumerable<SelectItem> Equipments { get; set; }
        
    }
}