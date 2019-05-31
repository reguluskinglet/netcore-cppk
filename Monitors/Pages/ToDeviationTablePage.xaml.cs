using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using NLog;
using RzdMonitors.Data;
using RzdMonitors.Data.Base;
using RzdMonitors.Data.Enums;
using RzdMonitors.Data.Reports;

namespace WpfMultiScreens.Pages
{
    /// <summary>
    /// Логика взаимодействия для ScheduleDeviationTablePage.xaml
    /// </summary>
    public partial class ToDeviationTablePage : IReportPage
    {
        private readonly string _depoName;
        public ScreenType ScreenType { get; set; } = ScreenType.ToDeviationTable;

        private readonly int _fullRefreshInterval;
        private const int PageSize = 9;
        private int _page = 1;
        private readonly Timer _pagingTimer;
        private ToDeviationTableDto _lastResult;
        private readonly object _pagingTimerLocker = new object();

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public ToDeviationTablePage(int refreshInterval)
        {
            InitializeComponent();

            _depoName = DataRepository.GetInstance().DepoName;
            _fullRefreshInterval = refreshInterval;

            //default 1 page
            _pagingTimer = new Timer { Interval = refreshInterval * 1000, Enabled = false };
            _pagingTimer.Elapsed += OnPagingTimer;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var timerHeader = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                TimeMark.Text = DateTime.Now.ToString("HH:mm");
                DateMark.Text = DateTime.Now.ToString("dd.MM.yyyy");
            }, Dispatcher);
            timerHeader.Start();
        }

        private void OnPagingTimer(object sender, ElapsedEventArgs e)
        {
            if (_lastResult != null)
            {
                _logger.Debug("OnPagingTimer()");
                lock (_pagingTimerLocker)
                {
                    var pages = _lastResult.Items.Length / PageSize + 1;

                    if (pages > 1)
                    {
                        _logger.Debug("pages > 1, need paging");

                        _page++;
                        if (_page > pages)
                        {
                            _page = 1;
                        }

                        _logger.Debug($"show page [{_page}]");
                        var pageData = _lastResult.Items.Skip((_page - 1) * PageSize).Take(PageSize);
                        Dispatcher.Invoke(() => { MainTable.ItemsSource = pageData; });
                    }
                }
            }
        }

        public void UpdateData()
        {
            _logger.Debug("UpdateData()");
            _pagingTimer.Stop();
            lock (_pagingTimerLocker)
            {
                try
                {
                    var res = DataRepository.GetInstance().GetToDeviationTable();

                    var printDepoName = _depoName;
                    if (_depoName.Length > 8)
                    {
                        printDepoName = _depoName.Substring(0, 7) + "…";
                    }

                    Dispatcher.Invoke(() =>
                    {
                        TableName.Text = "Расхождение фактического и планового графика для плановых мероприятий";
                        TableName.Foreground =
                            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff061e2c"));
                        HeaderLeft1.Text = $"Количество поездов на маршрутах: {res.TrainInTripCount}";
                        HeaderLeft2.Text = $"Количество поездов в депо {printDepoName}: {res.TrainDepoCount}";
                        HeaderRight.Text = $"Срывы графика: {res.GraphViolationCount}";
                        //1st page
                        MainTable.ItemsSource = res.Items.Take(PageSize);
                    });

                    _lastResult = res;
                    _page = 1;
                    //если есть еще страницы - надо запустить постраничный таймер
                    var pages = res.Items.Length / PageSize + 1;
                    _logger.Debug($"pages [{pages}]");
                    if (pages > 1)
                    {
                        //needs paging, calc proportional interval
                        var interval = _fullRefreshInterval / pages;
                        _pagingTimer.Interval = interval * 1000;
                        _pagingTimer.Start();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "UpdateData error");
                    Dispatcher.Invoke(() =>
                    {
                        TableName.Text = "ошибка обновления данных";
                        TableName.Foreground = new SolidColorBrush(Colors.Red);
                    });
                }
            }
        }
    }
}
