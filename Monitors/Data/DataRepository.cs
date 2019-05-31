using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using RzdMonitors.Data.PanelsConfig;
using RzdMonitors.Data.Reports;
using RzdPpk.Data.Reports;
using System.Web;
using NLog;
using WpfMultiScreens.Data.Reports;

namespace RzdMonitors.Data
{
    public class DataRepository
    {
        public string DepoName { get; set; }

        public int DepoStantionId { get; set; }

        private static DataRepository _instance;

        private readonly HttpClient _remoteClient;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private DataRepository()
        {
            _remoteClient = GetRemoteConnection();
        }

        public static DataRepository GetInstance()
        {
            return _instance ?? (_instance = new DataRepository());
        }

        public HttpClient GetRemoteConnection()
        {
            var apiKey = ConfigurationManager.AppSettings.Get("ApiKey");
            var dataServiceUrl = ConfigurationManager.AppSettings.Get("DataServiceUrl");
            var client = new HttpClient
            {
                BaseAddress = new Uri(dataServiceUrl)
            };
            client.DefaultRequestHeaders.Add("ApiKey", apiKey);
            return client;
        }

        public void SetDepoName()
        {
            var response = _remoteClient.GetAsync($"/api/TvPanels/GetDepoName?depoStantionId={DepoStantionId}").Result;

            string ret;

            if (response.IsSuccessStatusCode)
            {
                ret = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                _logger.Error($"SetDepoName non-success status code: {response.StatusCode}");
                throw new Exception("ошибка подключения к источнику данных");
            }

            DepoName = ret ?? throw new Exception("неверный DepoStantionId в настройках");
        }

        public int RegisterBox(TvBoxRegisterDto dto)
        {
            var response = _remoteClient.PostAsJsonAsync("/api/TvPanels/RegisterBox", dto).Result;

            if (response.IsSuccessStatusCode)
            {
                var id = response.Content.ReadAsAsync<int>().Result;

                return id;
            }
            else
            {
                _logger.Error($"RegisterBox non-success status code: {response.StatusCode}");
            }
            throw new Exception("ошибка подключения к источнику данных");
        }

        public List<TvPanelsDto> GetBoxPanels(int boxId)
        {
            var response = _remoteClient.GetAsync($"/api/TvPanels/GetBoxPanels?boxId={boxId}").Result;

            List<TvPanelsDto> list;

            if (response.IsSuccessStatusCode)
            {
                list = response.Content.ReadAsAsync<List<TvPanelsDto>>().Result;
            }
            else
            {
                _logger.Error($"GetBoxPanels non-success status code: {response.StatusCode}");
                throw new Exception("ошибка подключения к источнику данных");
            }

            return list;
        }

        public void AddBoxPanels(TvBoxPanelsDto dto)
        {
            var response = _remoteClient.PostAsJsonAsync("/api/TvPanels/AddBoxPanels", dto).Result;

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"AddBoxPanels non-success status code: {response.StatusCode}");
                throw new Exception("ошибка подключения к источнику данных");
            }
        }

        public void DeleteBoxPanels(TvBoxPanelsDto dto)
        {
            var response = _remoteClient.PostAsJsonAsync("/api/TvPanels/DeleteBoxPanels", dto).Result;

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"DeleteBoxPanels non-success status code: {response.StatusCode}");
                throw new Exception("ошибка подключения к источнику данных");
            }
        }

        public ScheduleDeviationTableDto GetScheduleDeviationTable()
        {
            var response = _remoteClient.GetAsync("/api/TvPanels/GetScheduleDeviationTable").Result;

            ScheduleDeviationTableDto ret;

            if (response.IsSuccessStatusCode)
            {
                ret = response.Content.ReadAsAsync<ScheduleDeviationTableDto>().Result;
            }
            else
            {
                _logger.Error($"GetScheduleDeviationTable non-success status code: {response.StatusCode}");
                throw new Exception("ошибка подключения к источнику данных");
            }

            return ret;
        }

        public ScheduleDeviationGraphDto GetScheduleDeviationGraph(DateTime start, DateTime end)
        {
            var reqParams = string.Format("start={0}&end={1}",
                Uri.EscapeDataString(start.ToString("yyyy-MM-ddTHH:mm:ss")),
                    Uri.EscapeDataString(end.ToString("yyyy-MM-ddTHH:mm:ss")));

            var response = _remoteClient.GetAsync("/api/TvPanels/GetScheduleDeviationGraph?"+ reqParams).Result;

            ScheduleDeviationGraphDto ret;

            if (response.IsSuccessStatusCode)
            {
                ret = response.Content.ReadAsAsync<ScheduleDeviationGraphDto>().Result;
            }
            else
            {
                _logger.Error($"GetScheduleDeviationGraph non-success status code: {response.StatusCode}");
                throw new Exception("ошибка подключения к источнику данных");
            }

            return ret;
        }

        public BrigadeScheduleDeviationTableDto GetBrigadeScheduleDeviationTable()
        {
            var response = _remoteClient.GetAsync("/api/TvPanels/GetBrigadeScheduleDeviationTable").Result;

            BrigadeScheduleDeviationTableDto ret;

            if (response.IsSuccessStatusCode)
            {
                ret = response.Content.ReadAsAsync<BrigadeScheduleDeviationTableDto>().Result;
            }
            else
            {
                _logger.Error($"GetBrigadeScheduleDeviationTable non-success status code: {response.StatusCode}");
                throw new Exception("ошибка подключения к источнику данных");
            }

            return ret;
        }

        public ToDeviationTableDto GetToDeviationTable()
        {
            var response = _remoteClient.GetAsync("/api/TvPanels/GetToDeviationTable").Result;

            ToDeviationTableDto ret;

            if (response.IsSuccessStatusCode)
            {
                ret = response.Content.ReadAsAsync<ToDeviationTableDto>().Result;
            }
            else
            {
                _logger.Error($"GetToDeviationTable non-success status code: {response.StatusCode}");
                throw new Exception("ошибка подключения к источнику данных");
            }

            return ret;
        }

        public CriticalMalfunctionsTableDto GetCriticalMalfunctionsTable()
        {
            var response = _remoteClient.GetAsync("/api/TvPanels/GetCriticalMalfunctionsTable").Result;

            CriticalMalfunctionsTableDto ret;

            if (response.IsSuccessStatusCode)
            {
                ret = response.Content.ReadAsAsync<CriticalMalfunctionsTableDto>().Result;
            }
            else
            {
                _logger.Error($"GetCriticalMalfunctionsTable non-success status code: {response.StatusCode}");
                throw new Exception("ошибка подключения к источнику данных");
            }

            return ret;
        }

        public TrainsInDepoMalfunctionTableDto GetTrainsInDepoDepoMalfunctionsTable(int depoStantionId)
        {
            var response = _remoteClient.GetAsync("/api/TvPanels/GetTrainsInDepoDepoMalfunctionsTable?depoStantionId="+depoStantionId).Result;

            TrainsInDepoMalfunctionTableDto ret;

            if (response.IsSuccessStatusCode)
            {
                ret = response.Content.ReadAsAsync<TrainsInDepoMalfunctionTableDto>().Result;
            }
            else
            {
                _logger.Error($"GetTrainsInDepoDepoMalfunctionsTable non-success status code: {response.StatusCode}");
                throw new Exception("ошибка подключения к источнику данных");
            }

            return ret;
        }

        public TrainsInDepoStatusTableDto GetTrainsInDepoStatusTable(int depoStantionId)
        {
            var response = _remoteClient.GetAsync("/api/TvPanels/GetTrainsInDepoStatusTable?depoStantionId=" + depoStantionId).Result;

            TrainsInDepoStatusTableDto ret;

            if (response.IsSuccessStatusCode)
            {
                ret = response.Content.ReadAsAsync<TrainsInDepoStatusTableDto>().Result;
            }
            else
            {
                _logger.Error($"GetTrainsInDepoStatusTable non-success status code: {response.StatusCode}");
                throw new Exception("ошибка подключения к источнику данных");
            }

            return ret;
        }

        public JournalsTableDto GetJournalsTable(int depoStantionId)
        {
            var response = _remoteClient.GetAsync("/api/TvPanels/GetJournalsTable?depoStantionId=" + depoStantionId).Result;

            JournalsTableDto ret;

            if (response.IsSuccessStatusCode)
            {
                ret = response.Content.ReadAsAsync<JournalsTableDto>().Result;
            }
            else
            {
                _logger.Error($"GetJournalsTable non-success status code: {response.StatusCode}");
                throw new Exception("ошибка подключения к источнику данных");
            }

            return ret;
        }
    }
}
