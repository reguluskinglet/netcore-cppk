using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Core.Other;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static Rzdppk.Core.Other.DevExtremeTableData;
using static Rzdppk.Core.Other.DevExtremeTableUtils;


namespace Rzdppk.Core.Services
{
    public class DictionaryService
    {

        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DictionaryService(ILogger logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ReportResponse> DictionaryTableManager(ReportRequest input)
        {
            if (!Int32.TryParse(input.ReportId, out var enumId))
                throw new ValidationException("reportId не int");
            ReportResponse result;

            switch (enumId)
            {
                case (int)DictionaryTableTableEnum.Stantion:
                    result = await GetStationsTable(input, _logger);
                    break;
                case (int)DictionaryTableTableEnum.Direction:
                    result = await GetDirectionsTable(input, _logger);
                    break;
                case (int)DictionaryTableTableEnum.Fault:
                    result = await GetFaultsTable(input, _logger);
                    break;
                //case (int)DictionaryTableTableEnum.Parking:
                    //result = await GetParkingsTable(input, _logger);
                    //break;
                case (int)DictionaryTableTableEnum.TripsWithDays:
                    result = await GetTripsWithDaysTable(input, _logger);
                    break;
                case (int)DictionaryTableTableEnum.DeviceFaults:
                    result = await GetDeviceFaultsTable(input, _logger);
                    break;
                default:
                    throw new ValidationException("Неизвестный reportId");
            }

            PagingFilteringSorting(input, result);
            return result;
        }


        /// <summary>
        /// Справочник станций
        /// </summary>
        private static async Task<ReportResponse> GetStationsTable(ReportRequest input, ILogger logger)
        {
            var sqlR = new StantionsRepository(logger);
            var result = new ReportResponse { Rows = new List<Row>() };

            var data = await sqlR.GetAll();

            result.Columns = new List<Column>
            {
                new Column("col0", "Название", "string", disableEdit: false),
                new Column("col1", "Тип", "string", disableEdit: false)
            };

            foreach (var item in data)
            {
                var row = new Row
                {
                    Id = new RowId(item.Id, 1),
                    HasItems = false.ToString(),
                    ParentId = null,
                    //Состав
                    Col0 = item.Name,
                    //Тип
                    Col1 = GetStringStationType(item.StantionType)
                };

                result.Rows.Add(row);
            }

            return result;
        }


        /// <summary>
        /// Справочник Направлений
        /// </summary>
        private static async Task<ReportResponse> GetDirectionsTable(ReportRequest input, ILogger logger)
        {
            var sqlR = new DirectionsRepository(logger);
            var result = new ReportResponse { Rows = new List<Row>() };

            var data = await sqlR.GetAll();

            result.Columns = new List<Column>
            {
                new Column("col0", "Название", "string", disableEdit: false),
            };

            foreach (var item in data)
            {
                var row = new Row
                {
                    Id = new RowId(item.Id, 1),
                    HasItems = false.ToString(),
                    ParentId = null,
                    //Имя
                    Col0 = item.Name,
                };

                result.Rows.Add(row);
            }

            return result;
        }


        /// <summary>
        /// Справочник неиправностей
        /// </summary>
        private static async Task<ReportResponse> GetFaultsTable(ReportRequest input, ILogger logger)
        {
            var sqlR = new FaultsRepository(logger);
            var result = new ReportResponse { Rows = new List<Row>() };

            var data = await sqlR.GetAll();

            //Тип из бд походу негде неюзается. Енум пустой
            result.Columns = new List<Column>
            {
                new Column("col0", "Имя", "string", disableEdit: false),
                new Column("col1", "Описание", "string", disableEdit: false),
            };

            foreach (var item in data)
            {
                var row = new Row
                {
                    Id = new RowId(item.Id, 1),
                    HasItems = false.ToString(),
                    ParentId = null,
                    //Состав
                    Col0 = item.Name,
                    //Тип
                    Col1 = item.Description
                };

                result.Rows.Add(row);
            }

            return result;
        }



        public async Task<DictionaryCrudResponse> FaultCrud(DictionaryCrudRequest input)
        {
            var data = input.Fault;
            if (data == null && input.IdToDelete == null)
                throw new ValidationException("Не распарсилось");

            var sqlR = new FaultsRepository(_logger);
            if (input?.IdToDelete != null)
            {
                await sqlR.Delete((int)input.IdToDelete);
                return new DictionaryCrudResponse { IsDeleted = true, Fault = data };
            }

            var all = await sqlR.GetAll();
            if (all.Any(x => x.Name.Equals(input.Fault.Name)))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            if (data?.Id == 0)
            {
                var item = await sqlR.Add(data);
                return new DictionaryCrudResponse { IsAdd = true, Fault = item };
            }
            else
            {
                //Неиспользуемая хуета этот тип фаулта
                data.FaultType = 0;
                var item = await sqlR.Update(data);
                return new DictionaryCrudResponse { IsUpdated = true, Fault = item };
            }
        }


        public async Task<DictionaryCrudResponse> DirectionCrud(DictionaryCrudRequest input)
        {
            var data = input.Direction;
            if (data == null && input.IdToDelete == null)
                throw new ValidationException("Не распарсилось");

            var sqlR = new DirectionsRepository(_logger);
            if (input?.IdToDelete != null)
            {
                await sqlR.Delete((int)input.IdToDelete);
                return new DictionaryCrudResponse { IsDeleted = true, Direction = data };
            }

            var all = await sqlR.GetAll();
            if (all.Any(x => x.Name.Equals(input.Direction.Name)))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            if (data?.Id == 0)
            {
                var item = await sqlR.Add(data);
                return new DictionaryCrudResponse { IsAdd = true, Direction = item };
            }
            else
            {
                var item = await sqlR.Update(data);
                return new DictionaryCrudResponse { IsUpdated = true, Direction = item };
            }
        }


        public async Task<DictionaryCrudResponse> StantionCrud(DictionaryCrudRequest input)
        {
            var data = input.Stantion;
            if (data == null && input.IdToDelete == null)
                throw new ValidationException("Не распарсилось");

            var sqlR = new StantionsRepository(_logger);
            if (input?.IdToDelete != null)
            {
                await sqlR.Delete((int)input.IdToDelete);
                return new DictionaryCrudResponse { IsDeleted = true, Stantion = data };
            }

            var all = await sqlR.GetAll();
            if (all.Any(x => x.Name.Equals(input.Stantion.Name)))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            if (data?.Id == 0)
            {
                var item = await sqlR.Add(data);
                return new DictionaryCrudResponse { IsAdd = true, Stantion = item };
            }
            else
            {
                var item = await sqlR.Update(data);
                return new DictionaryCrudResponse { IsUpdated = true, Stantion = item };
            }
        }



        /// <summary>
        /// Справочник Парковок 
        /// </summary>
        private static async Task<ReportResponse> GetParkingsTable(ReportRequest input, ILogger logger)
        {
            var sqlR = new ParkingRepository(logger);
            var sqlRStation = new StantionsRepository(logger);
            var result = new ReportResponse { Rows = new List<Row>() };

            var data = await sqlR.GetAll();

            result.Columns = new List<Column>
            {
                new Column("col0", "Название", "string", disableEdit: false),
                new Column("col1", "Описание", "string", disableEdit: false),
                new Column("col2", "Станция", "string", disableEdit: false),
            };

            foreach (var item in data)
            {
                var station = await sqlRStation.ById(item.StantionId);
                var stations = await sqlRStation.GetAll();
                stations = stations.Where(x => x.Id != item.StantionId).ToList();
                var avaibleStations = new List<StantionSimple>();
                foreach (var x in stations)
                {
                    avaibleStations.Add(new StantionSimple { StantionName = x.Name, StantionId = x.Id });
                }

                var row = new Row
                {
                    Id = new RowId(item.Id, 1),
                    HasItems = false.ToString(),
                    ParentId = null,
                    //Имя
                    Col0 = item.Name,
                    //Описание
                    Col1 = item.Description,
                    //Станция
                    Col2 = station.Name,
                    AdditionalProperty = new AdditionalProperty { AvaibleStations = avaibleStations, StationId = item.StantionId }
                };

                result.Rows.Add(row);
            }

            return result;
        }


        /*public async Task<DictionaryCrudResponse> ParkingCrud(DictionaryCrudRequest input)
        {
            var data = input.Parking;
            if (data == null && input.IdToDelete == null)
                throw new ValidationException("Не распарсилось");

            var sqlR = new ParkingRepository(_logger);
            if (input?.IdToDelete != null)
            {
                await sqlR.Delete((int)input.IdToDelete);
                return new DictionaryCrudResponse { IsDeleted = true, Parking = data };
            }

            var all = await sqlR.GetAll();
            if (data?.Id == 0 &&
                all.Any(x => x.Name.Equals(input.Parking.Name)))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            if (data?.Id == 0)
            {
                var item = await sqlR.Add(data);
                return new DictionaryCrudResponse { IsAdd = true, Parking = item };
            }
            else
            {
                var item = await sqlR.Update(data);
                return new DictionaryCrudResponse { IsUpdated = true, Parking = item };
            }
        }*/

        /// <summary>
        /// Справочник рейсов с днями 
        /// </summary>
        private static async Task<ReportResponse> GetTripsWithDaysTable(ReportRequest input, ILogger logger)
        {
            var sqlR = new TripsRepository(logger);
            var sqlRTripDays = new DayOfTripsRepoisitory(logger);
            var sqlRStationOnTrips = new StantionOnTripsRepository(logger);
            var sqlRStations = new StantionsRepository(logger);
            var result = new ReportResponse { Rows = new List<Row>() };

            var data = await sqlR.GetAll();

            result.Columns = new List<Column>
            {
                new Column("col0", "Имя", "string", disableEdit: false),
                new Column("col1", "Дни", "string"),
                new Column("col2", "Время отправления", "string"),
                new Column("col3", "Время прибытия", "string")
            };

            foreach (var item in data)
            {
                var stantionOnTrips = await sqlRStationOnTrips.ByTripId(item.Id);
                DateTime? startTime = null;
                DateTime? endTime = null;
                var days = (await sqlRTripDays.DaysByTripId(item.Id)).Select(x => x.Day).ToList();
                string stringDays = null;
                if (days.Any())
                {
                    foreach (var day in days)
                    {
                        if (stringDays == null)
                            stringDays = GetStringDayOfWeekShort(day);
                        else
                            stringDays = stringDays + ", " + GetStringDayOfWeekShort(day);
                    }
                }

                if (stantionOnTrips.Count >= 2)
                {
                    stantionOnTrips = stantionOnTrips.OrderBy(x => x.OutTime).ToList();
                    startTime = stantionOnTrips.First().OutTime;
                    endTime = stantionOnTrips.Last().InTime;
                }

                var row = new Row
                {
                    Id = new RowId(item.Id, 1),
                    HasItems = false.ToString(),
                    ParentId = null,
                    //Имя
                    Col0 = item.Name,
                    //Дни
                    Col1 = stringDays,
                    //"Время отправления
                    Col2 = startTime?.ToStringTimeOnly(),
                    //Время прибытия
                    Col3 = endTime?.ToStringTimeOnly(),
                    AdditionalProperty = new AdditionalProperty { StantionsWithTime = new List<StantionWithTimeSimple>() }
                };
                foreach (var stantionOnTrip in stantionOnTrips)
                {
                    var stationName = (await sqlRStations.ById(stantionOnTrip.StantionId))?.Name;
                    row.AdditionalProperty.StantionsWithTime.Add(
                        new StantionWithTimeSimple
                        {
                            StantionId = stantionOnTrip.StantionId,
                            StantionName = stationName,
                            InTime = stantionOnTrip.InTime,
                            OutTime = stantionOnTrip.OutTime
                        });
                }

                result.Rows.Add(row);
            }

            return result;
        }


        /// <summary>
        /// Можно менять только имя рейса
        /// </summary>
        public async Task<DictionaryCrudResponse> TripWithDaysCrud(DictionaryCrudRequest input)
        {
            var data = input.TripWithDays;
            if (data == null && input.IdToDelete == null)
                throw new ValidationException(Error.ParserError);

            var sqlR = new TripsRepository(_logger);

            if (input?.IdToDelete != null)
            {
                await sqlR.Delete((int)input.IdToDelete);
                return new DictionaryCrudResponse { IsDeleted = true, TripWithDays = data };
            }

            var all = await sqlR.GetAll();
            if (data?.Id==null && all.Any(x => x.Name.Equals(input.TripWithDays.Name)))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            if (data?.Id == 0)
            {
                var item = await sqlR.Add(data);
                return new DictionaryCrudResponse { IsAdd = true, TripWithDays = item };
            }
            else
            {
                var item = await sqlR.Update(data);
                return new DictionaryCrudResponse { IsUpdated = true, TripWithDays = item };
            }
        }


        /// <summary>
        /// Справочник DeviceFaults
        /// </summary>
        private static async Task<ReportResponse> GetDeviceFaultsTable(ReportRequest input, ILogger logger)
        {
            var sqlR = new DeviceFaultRepository(logger);
            var result = new ReportResponse { Rows = new List<Row>() };

            var data = await sqlR.GetAll();

            result.Columns = new List<Column>
            {
                new Column("col0", "Название", "string", disableEdit: false),
                new Column("col1", "Описание", "string", disableEdit: false),
            };

            foreach (var item in data)
            {
                var row = new Row
                {
                    Id = new RowId(item.Id, 1),
                    HasItems = false.ToString(),
                    ParentId = null,
                    //Имя
                    Col0 = item.Name,
                    //Описание
                    Col1 = item.Description,
                };

                result.Rows.Add(row);
            }

            return result;
        }

        /// <summary>
        /// КРУД DeviceFaults
        /// </summary>
        public async Task<DictionaryCrudResponse> DeviceFaultsCrud(DictionaryCrudRequest input)
        {
            var data = input.DeviceFault;
            if (data == null && input.IdToDelete == null)
                throw new ValidationException(Error.ParserError);

            var sqlR = new DeviceFaultRepository(_logger);
            if (input?.IdToDelete != null)
            {
                await sqlR.Delete((int)input.IdToDelete);
                return new DictionaryCrudResponse { IsDeleted = true, DeviceFault = data };
            }

            var all = await sqlR.GetAll();
            if (all.Any(x => x.Name.Equals(input.DeviceFault.Name)))
                throw new ValidationException(Error.AlreadyAddWithThisName);

            if (data?.Id == 0)
            {
                var item = await sqlR.Add(data);
                return new DictionaryCrudResponse { IsAdd = true, DeviceFault = item };
            }
            else
            {
                var item = await sqlR.Update(data);
                return new DictionaryCrudResponse { IsUpdated = true, DeviceFault = item };
            }
        }





        //Неспрашивайте меня нахуя так. 
        public class DictionaryCrudRequest : AbstractDictionaryCrudRequest
        {

        }

        public class DictionaryCrudResponse : AbstractDictionaryCrudResponce
        {

        }





        public abstract class AbstractDictionaryCrudRequest
        {
            public int? IdToDelete { get; set; }
            public Stantion Stantion { get; set; }
            public Direction Direction { get; set; }
            public Fault Fault { get; set; }
            public Parking Parking { get; set; }
            public Trip TripWithDays { get; set; }
            public DeviceFault DeviceFault { get; set; }
        }


        public abstract class AbstractDictionaryCrudResponce
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsDeleted { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsUpdated { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsAdd { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Stantion Stantion { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Direction Direction { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Fault Fault { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Parking Parking { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Trip TripWithDays { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public DeviceFault DeviceFault { get; set; }
        }
    }
}