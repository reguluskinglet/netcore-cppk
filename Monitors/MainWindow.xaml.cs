using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using NLog;
using RzdMonitors.Data;
using RzdMonitors.Data.Base;
using RzdMonitors.Data.Enums;
using RzdMonitors.Data.PanelsConfig;
using RzdMonitors.Util;
using WpfMultiScreens.Pages;
using WpfMultiScreens.Util;
using WpfMultiScreens.Windows;
using Timer = System.Timers.Timer;

namespace RzdMonitors
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// идентификатор системы для регистрации и получения конфигурации
        /// </summary>
        private int? _boxId;

        private Timer _timerConfig;
        private Timer _timerData;

        private readonly object _timerConfigLocker = new object();
        private readonly object _updateDataLocker = new object();

        private readonly int _refreshConfigInterval;
        private readonly int _refreshDataInterval;

        /// <summary>
        /// настройки для диаграмм - за сколько часов вперед/назад от текущего момента
        /// </summary>
        private readonly int _diagramHoursBack;
        private readonly int _diagramHoursForward;

        private readonly Dictionary<int, WindowPagesConfig> _monWinPagesDic = new Dictionary<int, WindowPagesConfig>();

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            InitializeComponent();

            //DepoName = ConfigurationManager.AppSettings.Get("DepoName");
            _refreshConfigInterval = int.Parse(ConfigurationManager.AppSettings.Get("ScreenConfigRefreshSec"));
            _refreshDataInterval = int.Parse(ConfigurationManager.AppSettings.Get("ScreenDataRefreshSec"));
            _diagramHoursBack = int.Parse(ConfigurationManager.AppSettings.Get("DiagramHoursBack"));
            _diagramHoursForward = int.Parse(ConfigurationManager.AppSettings.Get("DiagramHoursForward"));
            var depoStantionId = int.Parse(ConfigurationManager.AppSettings.Get("DepostantionId"));

            DataRepository.GetInstance().DepoStantionId = depoStantionId;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;

            //сначала покажем черные экраны по всем мониторам
            InitWindows();
            try
            {
                //зарегистрируемся на сервере
                RegisterBox();
                //определим наименование нашего депо
                DataRepository.GetInstance().SetDepoName();
                //поднимем данные и обновим панели
                OnTimerRefreshPanelsConfig(null, null);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "init error");
                MessageBox.Show($"ошибка инициализации: проверьте DataServiceUrl в настройках и доступность указанного сервиса по сети." + Environment.NewLine + ex.GetAllInnerExceptionMessages());
                ExitApp();
            }

            _timerConfig = new Timer {Interval = _refreshConfigInterval * 1000};
            _timerConfig.Elapsed += OnTimerRefreshPanelsConfig;
            _timerConfig.Start();

            _timerData = new Timer { Interval = _refreshDataInterval * 1000 };
            _timerData.Elapsed += OnTimerRefreshData;
            _timerData.Start();
        }

        private void RegisterBox()
        {
            //try read id from file
            var fileIdPath = Path.Combine(Directory.GetCurrentDirectory(), "boxId.txt");
            try
            {
                var text = File.ReadAllText(fileIdPath);
                _boxId = int.Parse(text);
            }
            catch (Exception)
            {
                //id not found
            }

            if (_boxId == null)
            {
                var machineName = Environment.MachineName;
                var box = new TvBoxRegisterDto
                {
                    Name = machineName,
                    PanelIds = ScreenHandler.GetScreens()
                };
                _boxId = DataRepository.GetInstance().RegisterBox(box);
                //save id to file
                File.WriteAllText(fileIdPath, _boxId.Value.ToString());
            }
        }

        private void OnTimerRefreshData(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (_updateDataLocker)
                {
                    foreach (var kv in _monWinPagesDic)
                    {
                        var num = kv.Key;
                        var config = kv.Value;
                        var idx = config.CurrentIdx + 1;
                        if (idx >= config.Pages.Length)
                        {
                            //cycle
                            idx = 0;
                        }
                        config.CurrentIdx = idx;
                        _logger.Debug($"screen {num}, idx = {idx}, next={config.CurrentIdx}");

                        if (config.Pages.Length > 1)
                        {
                            var curPage = config.Pages[idx];
                            _logger.Debug($"screen {num}, show page {idx} ({curPage})");
                            Dispatcher.Invoke(() => { config.Window.MainFrame.Navigate(curPage); });
                        }
                    }

                    UpdateData();
                }
            }
            catch (Exception)
            {
                //log?
            }
        }

        private void UpdateData()
        {
            Parallel.ForEach(_monWinPagesDic.Values.SelectMany(o => o.Pages).ToList(),
                (currentPage) =>
                {
                    _logger.Debug($"{currentPage} parallel call to UpdateData()");
                    currentPage.UpdateData();
                });
        }

        private void ExitApp()
        {
            ThreadStart ts = delegate ()
            {
                Dispatcher.BeginInvoke((Action)delegate ()
                {
                    Application.Current.Shutdown();
                });
            };
            Thread t = new Thread(ts);
            t.Start();
        }

        private void OnTimerRefreshPanelsConfig(object sender, ElapsedEventArgs e)
        {
            lock (_timerConfigLocker)
            {
                try
                {
                    if (_boxId.HasValue)
                    {
                        var data = DataRepository.GetInstance().GetBoxPanels(_boxId.Value);
                        if (data.Count == 0)
                        {
                            //либо нет регистрации либо кто-то ебнул все панели
                            _logger.Error("ошибка, получена пустая конфигурация");
                            MessageBox.Show("ошибка, получена пустая конфигурация");
                            ExitApp();
                        }

                        var curScreens = ScreenHandler.GetScreens();

                        if (data.Count != curScreens.Length)
                        {
                            if (data.Count > curScreens.Length)
                            {
                                _logger.Debug("screen count changed-");

                                var dto = new TvBoxPanelsDto
                                {
                                    BoxId = _boxId.Value,
                                    PanelIds = data.Where(o => !curScreens.Contains(o.Num)).Select(o => o.Num).ToArray()
                                };

                                DataRepository.GetInstance().DeleteBoxPanels(dto);
                            }
                            else
                            {
                                _logger.Debug("screen count changed+");
                                var dto = new TvBoxPanelsDto
                                {
                                    BoxId = _boxId.Value,
                                    PanelIds = curScreens.Where(o => !data.Select(p => p.Num).Contains(o)).ToArray()
                                };

                                DataRepository.GetInstance().AddBoxPanels(dto);
                            }
                        }
                        //перезапросим актуальные данные после изменения числа мониторов
                        data = DataRepository.GetInstance().GetBoxPanels(_boxId.Value);

                        UpdatePanelsData(data);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "OnTimerRefreshPanelsConfig error");
                }
            }
        }

        private void UpdatePanelsData(IReadOnlyCollection<TvPanelsDto> data)
        {
            var curScreens = ScreenHandler.GetScreens();

            var isEqual = true;
            foreach (var screen in curScreens)
            {
                var screenItem = data.SingleOrDefault(o => o.Num == screen) ?? new TvPanelsDto
                {
                    Num = screen,
                    Types = new[] {ScreenType.None}
                };

                var currentItem = new WindowPagesConfig
                {
                    ScreenNum = screen,
                    Pages = new IReportPage[] { }
                };

                if (_monWinPagesDic.ContainsKey(screen))
                {
                    currentItem = _monWinPagesDic[screen];
                }

                isEqual = currentItem.Pages.Select(o => o.ScreenType).SequenceEqual(screenItem.Types);
                if (!isEqual)
                    break;
            }

            if (!isEqual)
            {
                lock (_updateDataLocker)
                {
                    Dispatcher.Invoke(RemoveAllWindows);

                    foreach (var screen in curScreens)
                    {
                        var screenItem = data.SingleOrDefault(o => o.Num == screen) ?? new TvPanelsDto
                        {
                            Num = screen,
                            Types = new[] {ScreenType.None}
                        };

                        WindowPagesConfig config;
                        var pages = new List<IReportPage>();
                        Dispatcher.Invoke(() =>
                        {
                            foreach (var type in screenItem.Types)
                            {
                                //todo factory?
                                IReportPage page;
                                switch (type)
                                {
                                    case ScreenType.ScheduleDeviationTable:
                                        page = new ScheduleDeviationTablePage(_refreshDataInterval);
                                        break;
                                    case ScreenType.ScheduleDeviationGraph:
                                        page = new ScheduleDeviationGraphPage(_refreshDataInterval, _diagramHoursBack, _diagramHoursForward);
                                        break;
                                    case ScreenType.BrigadeScheduleDeviationTable:
                                        page = new BrigadeScheduleDeviationTablePage(_refreshDataInterval);
                                        break;
                                    case ScreenType.ToDeviationTable:
                                        page = new ToDeviationTablePage( _refreshDataInterval);
                                        break;
                                    case ScreenType.CriticalMalfunctionsTable:
                                        page = new CriticalMalfunctionsTablePage(_refreshDataInterval);
                                        break;
                                    case ScreenType.TrainsInDepoMalfunctionsTable:
                                        page = new TrainsInDepoMalfunctionsTablePage(_refreshDataInterval);
                                        break;
                                    case ScreenType.TrainsInDepoStatusTable:
                                        page = new TrainsInDepoStatusTablePage(_refreshDataInterval);
                                        break;
                                    case ScreenType.JournalsTable:
                                        page = new JournalsTablePage(_refreshDataInterval);
                                        break;
                                    default:
                                        page = new StartPage();
                                        break;
                                }

                                pages.Add(page);
                            }

                            config = new WindowPagesConfig
                            {
                                ScreenNum = screen,
                                CurrentIdx = 0,
                                Window = new ScreenWindow(),
                                Pages = pages.ToArray()
                            };

                            _monWinPagesDic[screen] = config;

                            ShowOnMonitor(screen, config.Window);
                            //navigate to first page
                            config.Window.MainFrame.Navigate(config.Pages[0]);
                        });
                    }

                    Task.Run(() =>
                    {
                        UpdateData();
                    });
                }
            }
        }

        private void RemoveAllWindows()
        {
            foreach (var kv in _monWinPagesDic)
            {
                kv.Value.Window.Close();
                //dispose pages?
            }
            _monWinPagesDic.Clear();
        }

        private void InitWindows()
        {
            var curScreens = ScreenHandler.GetScreens();
            foreach (var screen in curScreens)
            {
                var win = new ScreenWindow();
                var config = new WindowPagesConfig
                {
                    ScreenNum = screen,
                    CurrentIdx = 0,
                    Window = win,
                    Pages = new IReportPage[] {new StartPage()}
                };
                _monWinPagesDic.Add(screen, config);

                Dispatcher.Invoke(() =>
                {
                    ShowOnMonitor(screen, win);
                    win.MainFrame.Navigate(config.Pages[0]);
                });
            }
        }

        private void ShowOnMonitor(int monitorId, Window window)
        {
            //if (_monitorWindowDic.ContainsKey(monitorId))
            //{
            //    var oldWindow = _monitorWindowDic[monitorId];
            //    oldWindow.Close(); //hide?
            //    _monitorWindowDic.Remove(monitorId);
            //}

            var screen = ScreenHandler.GetScreen(monitorId);
            //var currentScreen = ScreenHandler.GetCurrentScreen(this);
            //window.WindowState = WindowState.Normal;
            window.Left = screen.WorkingArea.Left;
            window.Top = screen.WorkingArea.Top;
            //window.Width = screen.WorkingArea.Width;
            //window.Height = screen.WorkingArea.Height;
            //window.Loaded += Window_Loaded;
            window.SourceInitialized += (snd, arg) => window.WindowState = WindowState.Maximized;
            window.Show();
            //window.WindowState = WindowState.Maximized;
        }

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (sender is Window senderWindow) senderWindow.WindowState = WindowState.Maximized;
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ExitApp();
        }
    }
}
