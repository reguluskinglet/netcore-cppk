using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using NLog;
using Rzdmonitors.Util;
using RzdMonitors.Data;
using RzdMonitors.Data.Base;
using RzdMonitors.Data.Enums;
using RzdMonitors.Data.Reports;
using RzdMonitors.Util;

namespace WpfMultiScreens.Pages
{
    /// <summary>
    /// Логика взаимодействия для ScheduleDeviationGraphPage.xaml
    /// </summary>
    public partial class ScheduleDeviationGraphPage : IReportPage
    {
        private DateTime _minDate;
        private DateTime _maxDate;
        private readonly string _depoName;
        
        public ScreenType ScreenType { get; set; } = ScreenType.ScheduleDeviationGraph;
        
        private ScheduleDeviationGraphDto _lastResult;
        private readonly object _pagingTimerLocker = new object();
        private readonly Timer _pagingTimer;
        private const int PageSize = 5;
        private int _page = 1;
        private readonly int _fullRefreshInterval;
        private readonly int _hoursBack;
        private readonly int _hoursForward;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public ScheduleDeviationGraphPage(int refreshInterval, int hoursBack, int hoursForward)
        {
            InitializeComponent();

            _depoName = DataRepository.GetInstance().DepoName;
            _fullRefreshInterval = refreshInterval;
            _hoursBack = hoursBack;
            _hoursForward = hoursForward;

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

                        DrawCurrentPage();
                    }
                }
            }
        }

        public void DrawCurrentPage()
        {
            if (_lastResult != null)
            {
                _logger.Debug($"show page [{_page}]");

                var pageData = _lastResult.Items.Skip((_page - 1) * PageSize).Take(PageSize);
                Dispatcher.Invoke(() =>
                {
                    DrawTable(pageData.ToList());
                });
            }
        }

        #region test
        //private List<RouteDiffRecordDto> GetData()
        //{
        //    var recordList = new List<RouteDiffRecordDto>
        //    {
        //        new RouteDiffRecordDto
        //        {
        //            RouteName = "666/566",
        //            TrainName = "70700",
        //            EventsPlan = new List<TimedTaskDto>
        //            {
        //                new TimedTaskDto
        //                {
        //                    Name = "хуйпесда",
        //                    Start = DateTime.Parse("08.07.2018 11:40"),
        //                    End = null,
        //                    //End = DateTime.Parse("08.07.2018 14:40"),
        //                    BgColor = "#00000000",
        //                    FgColor = "#ffff0000"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "666/566",
        //                    Start = DateTime.Parse("08.07.2018 13:30"),
        //                    End = DateTime.Parse("08.07.2018 14:35"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "TO",
        //                    Start = DateTime.Parse("08.07.2018 14:50"),
        //                    End = DateTime.Parse("08.07.2018 15:05"),
        //                    BgColor = "#fffffe85",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "7777",
        //                    Start = DateTime.Parse("08.07.2018 15:20"),
        //                    End = DateTime.Parse("08.07.2018 16:11"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                }
        //            },
        //            EventsFact = new List<TimedTaskDto>
        //            {
        //                new TimedTaskDto
        //                {
        //                    Name = "666/566",
        //                    Start = DateTime.Parse("08.07.2018 13:35"),
        //                    End = DateTime.Parse("08.07.2018 14:40"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "TO",
        //                    Start = DateTime.Parse("08.07.2018 14:50"),
        //                    End = DateTime.Parse("08.07.2018 15:05"),
        //                    BgColor = "#fffffe85",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "7777",
        //                    Start = DateTime.Parse("08.07.2018 15:30"),
        //                    End = DateTime.Parse("08.07.2018 16:21"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                }
        //            }
        //        },
        //        new RouteDiffRecordDto
        //        {
        //            RouteName = "777/566",
        //            TrainName = "70800",
        //            EventsPlan = new List<TimedTaskDto>
        //            {
        //                new TimedTaskDto
        //                {
        //                    Name = "777/566",
        //                    Start = DateTime.Parse("08.07.2018 13:30"),
        //                    End = DateTime.Parse("08.07.2018 14:35"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "TO",
        //                    Start = DateTime.Parse("08.07.2018 14:50"),
        //                    End = DateTime.Parse("08.07.2018 15:05"),
        //                    BgColor = "#fffffe85",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "8888",
        //                    Start = DateTime.Parse("08.07.2018 15:20"),
        //                    End = DateTime.Parse("08.07.2018 16:11"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                }
        //            },
        //            EventsFact = new List<TimedTaskDto>
        //            {
        //                new TimedTaskDto
        //                {
        //                    Name = "777/566",
        //                    Start = DateTime.Parse("08.07.2018 13:35"),
        //                    End = DateTime.Parse("08.07.2018 14:40"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "TO",
        //                    Start = DateTime.Parse("08.07.2018 14:50"),
        //                    End = DateTime.Parse("08.07.2018 15:05"),
        //                    BgColor = "#fffffe85",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "8888",
        //                    Start = DateTime.Parse("08.07.2018 15:30"),
        //                    End = DateTime.Parse("08.07.2018 16:21"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                }
        //            }
        //        },
        //        new RouteDiffRecordDto
        //        {
        //            RouteName = "888/566",
        //            TrainName = "70900",
        //            EventsPlan = new List<TimedTaskDto>
        //            {
        //                new TimedTaskDto
        //                {
        //                    Name = "888/566",
        //                    Start = DateTime.Parse("08.07.2018 13:30"),
        //                    End = DateTime.Parse("08.07.2018 14:35"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "TO",
        //                    Start = DateTime.Parse("08.07.2018 14:50"),
        //                    End = DateTime.Parse("08.07.2018 15:05"),
        //                    BgColor = "#fffffe85",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "9999",
        //                    Start = DateTime.Parse("08.07.2018 15:20"),
        //                    End = DateTime.Parse("08.07.2018 16:11"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                }
        //            },
        //            EventsFact = new List<TimedTaskDto>
        //            {
        //                new TimedTaskDto
        //                {
        //                    Name = "888/566",
        //                    Start = DateTime.Parse("08.07.2018 13:35"),
        //                    End = DateTime.Parse("08.07.2018 14:40"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "TO",
        //                    Start = DateTime.Parse("08.07.2018 14:50"),
        //                    End = DateTime.Parse("08.07.2018 15:05"),
        //                    BgColor = "#fffffe85",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "9999",
        //                    Start = DateTime.Parse("08.07.2018 15:30"),
        //                    End = DateTime.Parse("08.07.2018 16:21"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                }
        //            }
        //        },
        //        new RouteDiffRecordDto
        //        {
        //            RouteName = "999/566",
        //            TrainName = "70600",
        //            EventsPlan = new List<TimedTaskDto>
        //            {
        //                new TimedTaskDto
        //                {
        //                    Name = "999/566",
        //                    Start = DateTime.Parse("08.07.2018 13:30"),
        //                    End = DateTime.Parse("08.07.2018 14:35"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "TO",
        //                    Start = DateTime.Parse("08.07.2018 14:50"),
        //                    End = DateTime.Parse("08.07.2018 15:05"),
        //                    BgColor = "#fffffe85",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "6666",
        //                    Start = DateTime.Parse("08.07.2018 15:20"),
        //                    End = DateTime.Parse("08.07.2018 16:11"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                }
        //            },
        //            EventsFact = new List<TimedTaskDto>
        //            {
        //                new TimedTaskDto
        //                {
        //                    Name = "999/566",
        //                    Start = DateTime.Parse("08.07.2018 13:35"),
        //                    End = DateTime.Parse("08.07.2018 14:40"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "TO",
        //                    Start = DateTime.Parse("08.07.2018 14:50"),
        //                    End = DateTime.Parse("08.07.2018 15:05"),
        //                    BgColor = "#fffffe85",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "6666",
        //                    Start = DateTime.Parse("08.07.2018 15:30"),
        //                    End = DateTime.Parse("08.07.2018 16:21"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                }
        //            }
        //        },
        //        new RouteDiffRecordDto
        //        {
        //            RouteName = "555/566",
        //            TrainName = "70500",
        //            EventsPlan = new List<TimedTaskDto>
        //            {
        //                new TimedTaskDto
        //                {
        //                    Name = "555/566",
        //                    Start = DateTime.Parse("08.07.2018 13:30"),
        //                    End = DateTime.Parse("08.07.2018 14:35"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "TO",
        //                    Start = DateTime.Parse("08.07.2018 14:50"),
        //                    End = DateTime.Parse("08.07.2018 15:05"),
        //                    BgColor = "#fffffe85",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "5555",
        //                    Start = DateTime.Parse("08.07.2018 15:20"),
        //                    End = DateTime.Parse("08.07.2018 16:11"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                }
        //            },
        //            EventsFact = new List<TimedTaskDto>
        //            {
        //                new TimedTaskDto
        //                {
        //                    Name = "555/566",
        //                    Start = DateTime.Parse("08.07.2018 13:35"),
        //                    End = DateTime.Parse("08.07.2018 14:40"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "TO",
        //                    Start = DateTime.Parse("08.07.2018 14:50"),
        //                    End = DateTime.Parse("08.07.2018 15:05"),
        //                    BgColor = "#fffffe85",
        //                    FgColor = "#ff3c5466"
        //                },
        //                new TimedTaskDto
        //                {
        //                    Name = "5555",
        //                    Start = DateTime.Parse("08.07.2018 15:30"),
        //                    End = DateTime.Parse("08.07.2018 16:21"),
        //                    BgColor = "#d0c7e7ff",
        //                    FgColor = "#ff3c5466"
        //                }
        //            }
        //        }
        //    };

        //    return recordList;
        //}

        //private void UpdateTimeRange()
        //{
        //    var nowDate = DateTime.Now;
        //    _minDate = nowDate - new TimeSpan(3, 0, 0);
        //    _maxDate = nowDate + new TimeSpan(3, 0, 0);
        //}
#endregion

        public void UpdateData()
        {
            _logger.Debug("UpdateData()");
            _pagingTimer.Stop();
            lock (_pagingTimerLocker)
            {
                try
                {
                    var nowDate = DateTime.Now;
                    _minDate = nowDate - new TimeSpan(_hoursBack, 0, 0);
                    _maxDate = nowDate + new TimeSpan(_hoursForward, 0, 0);

                    var res = DataRepository.GetInstance().GetScheduleDeviationGraph(_minDate, _maxDate);

                    _lastResult = res;
                    _page = 1;

                    var printDepoName = _depoName;
                    if (_depoName.Length > 8)
                    {
                        printDepoName = _depoName.Substring(0, 7) + "…";
                    }

                    Dispatcher.Invoke(() =>
                    {
                        TableName.Text = "Расхождение фактического и планового графика";
                        TableName.Foreground =
                            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff061e2c"));
                        HeaderLeft1.Text = $"Количество поездов на маршрутах: {res.TrainInTripCount}";
                        HeaderLeft2.Text = $"Количество поездов в депо {printDepoName}: {res.TrainDepoCount}";
                        HeaderRight.Text = $"Срывы графика: {res.GraphViolationCount}";
                        //1st page
                        DrawCurrentPage();
                    });

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

        //private void OnLoaded(object sender, RoutedEventArgs e)
        //{
        //    _maxDate = DateTime.Parse("2018-07-08 17:30:00");
        //    _minDate = DateTime.Parse("2018-07-08 11:30:00");

        //    var timerHeader = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
        //    {
        //        TimeMark.Text = DateTime.Now.ToString("HH:mm");
        //        DateMark.Text = DateTime.Now.ToString("dd.MM.yyyy");
        //        var trainInTripCount = 5;
        //        var trainDepoCount = 1;
        //        var graphViolationCount = 1;
        //        var printDepoName = _depoName;
        //        if (_depoName.Length > 8)
        //        {
        //            printDepoName = _depoName.Substring(0, 7) + "…";
        //        }

        //        TableName.Text = "Расхождение фактического и планового графика";
        //        HeaderLeft1.Text = $"Количество поездов на маршрутах: {trainInTripCount}";
        //        HeaderLeft2.Text = $"Количество поездов в депо {printDepoName}: {trainDepoCount}";
        //        HeaderRight.Text = $"Срывы графика: {graphViolationCount}";
        //    }, Dispatcher);
        //    timerHeader.Start();

        //    var recordList = GetData();

        //    DrawTable(recordList);

        //    var timerTable = new DispatcherTimer(new TimeSpan(0, 0, 5), DispatcherPriority.Normal, delegate
        //    {
        //        _minDate += new TimeSpan(0, 15, 0);
        //        _maxDate += new TimeSpan(0, 15, 0);
        //        //UpdateTimeRange();
        //        DrawTable(recordList);
        //    }, Dispatcher);
        //    timerTable.Start();
        //}

        private void DrawTable(List<RouteDiffRecordDto> recordList)
        {
            var grid = new Grid { ShowGridLines = false };

            AddTableHeaders(grid);
            //2 rows per item
            int row = 1;
            foreach (var record in recordList)
            {
                //borders
                AddRecordBorders(grid, row);
                //plan
                AddTasksToPanel(grid, TaskPanelType.Plan, row++, record);
                //fact
                AddTasksToPanel(grid, TaskPanelType.Fact, row++, record);
            }

            //replace grid on window
            var cont = MainGridContainer;
            if (cont != null)
            {
                cont.Child = grid;
            }
        }

        private void AddTableHeaders(Grid grid)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(142) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(182) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(158) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1211) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
            //table headers
            for (int l = 2; l <= 3; l++)
            {
                var brdh1 = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff3c5466")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffffff")),
                    BorderThickness = new Thickness(0, 0, 0, 0)
                };
                grid.Children.Add(brdh1);
                Grid.SetRow(brdh1, 0);
                Grid.SetColumn(brdh1, l);
            }
            var columns = new List<string> { "Маршрут", "Поезд" };
            int i = 0;
            foreach (var colname in columns)
            {
                //header border
                var thickness = new Thickness(1, 0, 1, 0);
                if (i == 0)
                    thickness = new Thickness(0, 0, 1, 0);

                var brdh = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff3c5466")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffffff")),
                    BorderThickness = thickness
                };
                grid.Children.Add(brdh);
                Grid.SetRow(brdh, 0);
                Grid.SetColumn(brdh, i);

                var txt1 = new TextBlock
                {
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 27,
                    FontFamily = new FontFamily("AvantGardeGothicC"),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = colname
                };
                grid.Children.Add(txt1);
                Grid.SetRow(txt1, 0);
                Grid.SetColumn(txt1, i);

                i++;
            }
            var brdh2 = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff3c5466")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffffff")),
                BorderThickness = new Thickness(1, 0, 0, 0)
            };
            grid.Children.Add(brdh2);
            Grid.SetRow(brdh2, 0);
            Grid.SetColumn(brdh2, 2);
            //end table headers

            //hours timeline
            var ganntHeader = new GanttRowPanel
            {
                MinDate = _minDate,
                MaxDate = _maxDate
            };

            var tmpDate = _minDate;
            while (tmpDate <= _maxDate)
            {
                tmpDate = tmpDate.AddHours(1);

                var hour = tmpDate.Hour;
                var tmpDate1 = ChangeTime(tmpDate, hour, 0, 0, 0);
                var txtHour = new TextBlock
                {
                    Foreground = new SolidColorBrush(Colors.White),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 27,
                    FontFamily = new FontFamily("AvantGardeGothicC"),
                    Text = hour.ToString(),
                };
                txtHour.SetValue(GanttRowPanel.StartDateProperty, tmpDate1 - new TimeSpan(0, 15, 0));
                txtHour.SetValue(GanttRowPanel.EndDateProperty, tmpDate1 + new TimeSpan(0, 15, 0));
                ganntHeader.Children.Add(txtHour);
            }
            //
            grid.Children.Add(ganntHeader);
            Grid.SetRow(ganntHeader, 0);
            Grid.SetColumn(ganntHeader, 3);
            //end hours timeline
        }

        private void AddRecordBorders(Grid grid, int row)
        {
            //границы первых двух ячеек
            for (int l = 0; l <= 1; l++)
            {
                var brd1 = new Border
                {
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff11191e")),
                    BorderThickness = new Thickness(1)
                };
                grid.Children.Add(brd1);
                Grid.SetRow(brd1, row);
                Grid.SetColumn(brd1, l);
                Grid.SetRowSpan(brd1, 2);
            }
            //граница ячейки с таймлайном
            var brd3 = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff11191e")),
                BorderThickness = new Thickness(1)
            };
            grid.Children.Add(brd3);
            Grid.SetRow(brd3, row);
            Grid.SetColumn(brd3, 2);
            Grid.SetRowSpan(brd3, 2);
            Grid.SetColumnSpan(brd3, 2);

            //тонкая сераця граница между план/факт
            var brd4 = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffb7b7b7")),
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
            grid.Children.Add(brd4);
            Grid.SetRow(brd4, row);
            Grid.SetColumn(brd4, 0);
            Grid.SetColumnSpan(brd4, 4);
        }

        private void AddTasksToPanel(Grid grid, TaskPanelType type, int row, RouteDiffRecordDto record)
        {
            //plan
            string name;
            List<TimedTaskDto> tasks;
            switch (type)
            {
                case TaskPanelType.Plan:
                    name = "план";
                    tasks = record.EventsPlan;
                    break;
                case TaskPanelType.Fact:
                    name = "факт";
                    tasks = record.EventsFact;
                    break;
                default:
                    throw new Exception("unknown TaskPanelType");
            }

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60, GridUnitType.Pixel) });

            var txtc1 = new TextBlock
            {
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff3c5466")),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(record.BgColor)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 27,
                FontFamily = new FontFamily("AvantGardeGothicC"),
                Text = record.RouteName
            };
            grid.Children.Add(txtc1);
            Grid.SetRow(txtc1, row);
            Grid.SetColumn(txtc1, 0);

            var txtc2 = new TextBlock
            {
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff3c5466")),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(record.BgColor)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 27,
                FontFamily = new FontFamily("AvantGardeGothicC"),
                Text = record.TrainName
            };
            grid.Children.Add(txtc2);
            Grid.SetRow(txtc2, row);
            Grid.SetColumn(txtc2, 1);

            var txtc3 = new TextBlock
            {
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff3c5466")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 27,
                FontFamily = new FontFamily("AvantGardeGothicC"),
                Text = name
            };
            grid.Children.Add(txtc3);
            Grid.SetRow(txtc3, row);
            Grid.SetColumn(txtc3, 2);

            var ganntRow = new GanttRowPanel
            {
                MinDate = _minDate,
                MaxDate = _maxDate
            };
            foreach (var task in tasks)
            {
                if (task.Start > _maxDate)
                    continue;
                if (task.End.HasValue && task.End.Value < _minDate)
                    continue;

                var brd = new Border
                {
                    Height = 45,
                    Background = new SolidColorBrush((Color) ColorConverter.ConvertFromString(task.BgColor))
                };
                if (task.BorderColor != null)
                {
                    brd.BorderThickness = new Thickness(2);
                    brd.BorderBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString(task.BorderColor));
                }
                brd.SetValue(GanttRowPanel.StartDateProperty, task.Start);
                brd.SetValue(GanttRowPanel.EndDateProperty, task.End);
                //
                var txt = new TextBlock
                {
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(task.FgColor)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 30,
                    FontFamily = new FontFamily("AvantGardeGothicC"),
                    Text = task.Name
                };
                brd.Child = txt;
                ganntRow.Children.Add(brd);
            }

            var tmpDate1 = _minDate;
            while (tmpDate1 + new TimeSpan(1, 0, 0) <= _maxDate)
            {
                tmpDate1 = tmpDate1.AddHours(1);

                var hour = tmpDate1.Hour;
                var tmpDate2 = ChangeTime(tmpDate1, hour, 0, 0, 0);
                var hourLine = new Line
                {
                    X1 = 0,
                    X2 = 0,
                    Y1 = 0,
                    Y2 = 60,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffb7b7b7")),
                    Stretch = Stretch.Fill,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 5, 5 }
                };
                hourLine.SetValue(GanttRowPanel.StartDateProperty, tmpDate2);
                hourLine.SetValue(GanttRowPanel.EndDateProperty, tmpDate2);
                ganntRow.Children.Add(hourLine);
            }
            grid.Children.Add(ganntRow);
            Grid.SetRow(ganntRow, row);
            Grid.SetColumn(ganntRow, 3);
        }

        private static DateTime ChangeTime(DateTime dateTime, int hours, int minutes, int seconds, int milliseconds)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                hours,
                minutes,
                seconds,
                milliseconds,
                dateTime.Kind);
        }
    }
}
