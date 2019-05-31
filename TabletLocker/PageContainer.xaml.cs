using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media;
using Dapper;
using NLog;
using Rzdppk.Model.Enums;
using TabletLocker.CellController;
using TabletLocker.Db;
using TabletLocker.Db.Interfaces;
using TabletLocker.Enums;
using TabletLocker.Model;
using Timer = System.Timers.Timer;

namespace TabletLocker
{
    /// <summary>
    /// Логика взаимодействия для PageContainer.xaml
    /// </summary>
    public partial class PageContainer : Window
    {
        private ICellsController _controller;
        private SerialPort _serialPort;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        //map of open/closed cells
        private Dictionary<int, bool> _dicCells = new Dictionary<int, bool>();

        private readonly string _barcodeScannerPort = ConfigurationManager.AppSettings["BarcodeScannerPort"];
        private readonly string _cellControllerIp = ConfigurationManager.AppSettings["CellControllerIp"];
        private readonly int _cellControllerPort = int.Parse(ConfigurationManager.AppSettings["CellControllerPort"]);
        private readonly int _dbSyncInterval = int.Parse(ConfigurationManager.AppSettings["DbSyncIntervalSeconds"]);

        public int? OpenedCell { get; private set; }
        public User CurrentUser { get; set; }
        public Device CurrentDevice { get; set; }
        public DeviceOperation? CurrentOperation { get; set; }
        private readonly Timer _mainMenuReturnTimer;

        private MainPage _mainPage;
        private AdminPage _adminPage;

        private readonly ITaskRepository _taskRepository = new TaskRepository();
        private readonly IDeviceRepository _deviceRepository = new DeviceRepository();
        private readonly IUserRepository _userRepository = new UserRepository();
        private readonly IOperationRepository _operationRepository = new OperationRepository();
        private readonly IDeviceFaultRepository _deviceFaultRepository = new DeviceFaultRepository();

        public PageContainer()
        {
            _logger.Info("application start");
            //var cultureInfo = new CultureInfo("ru-RU");

            //CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            //CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("MainMenuReturnTimeout"), out var interval))
            {
                interval = 60;
            }

            _mainMenuReturnTimer = new Timer
            {
                Interval = interval * 1000,
                Enabled = false
            };
            _mainMenuReturnTimer.Elapsed += OnMainMenuReturnTimer;

            InitializeComponent();
            StateBegin();
        }

        //state machine
        private AppState _currentState;
        private AppState _prevState;

        private void SetState(AppState value)
        {
            _prevState = _currentState;
            _currentState = value;
            _logger.Debug($"state transition: {_prevState} -> {_currentState}");
        }

        public void StateBegin()
        {
            SetState(AppState.Begin);

            _mainPage = new MainPage(this);
        }

        public void StateInit()
        {
            SetState(AppState.Init);

            bool.TryParse(ConfigurationManager.AppSettings.Get("InitialSyncMandatory"), out var mandatory);

            try
            {
                SqlMapper.AddTypeMap(typeof(DateTime), DbType.DateTime2);
                //обязательная синхронизация баз данных при старте приложения
                try
                {
                    DbSyncronizer.GetInstance().SyncDatabases();
                }
                catch (Exception)
                {
                    if (mandatory)
                    {
                        throw;
                    }
                }

                //инициализация железа
                InitBarcodeScanner();
                InitCellsController();
                //таймер для периодической синхронизации баз данных
                InitDbSyncTimer();

                StateMainMenu();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "init error");
                StateInitError(ex is SyncException ? "ошибка синхр. бд" : ex.Message);
            }
        }

        public void StateInitError(string message)
        {
            SetState(AppState.InitError);

            Dispatcher.Invoke(() =>
            {
                _mainPage.BtnBarcodeEnter.IsEnabled = true;
                _mainPage.BtnReport.IsEnabled = false;
                _mainPage.BtnReturn.Visibility = Visibility.Hidden;
                _mainPage.BtnTake.IsEnabled = false;
                _mainPage.BtnBack.Visibility = Visibility.Hidden;
                _mainPage.TroubleInputWindow.Visibility = Visibility.Hidden;
                _mainPage.UncloseableMsgBox.Visibility = Visibility.Hidden;
                //
                _mainPage.MsgBoxTitle.Text = "Ошибка инициализации";
                _mainPage.MsgBoxMessage.Text = message;
                _mainPage.MsgBox.Visibility = Visibility.Visible;
                _mainPage.MainCanvasBlur.Radius = 20;

                MainFrame.NavigationService.Navigate(_mainPage);
            });
        }

        public void StateMainError(string message)
        {
            SetState(AppState.MainError);

            Dispatcher.Invoke(() =>
            {
                _mainPage.MsgBoxTitle.Text = "Ошибка";
                _mainPage.MsgBoxMessage.Text = message;
                _mainPage.MsgBox.Visibility = Visibility.Visible;
                _mainPage.MainCanvasBlur.Radius = 20;
                _mainPage.TroubleInputWindow.Visibility = Visibility.Hidden;
                _mainPage.UncloseableMsgBox.Visibility = Visibility.Hidden;

                MainFrame.NavigationService.Navigate(_mainPage);
            });
        }

        public void StateMainMenu()
        {
            SetState(AppState.MainMenu);
            CurrentUser = null;
            CurrentDevice = null;
            CurrentOperation = null;

            Dispatcher.Invoke(() =>
            {
                _mainPage.MsgBox.Visibility = Visibility.Hidden;
                _mainPage.AdminLoginButton.Visibility = Visibility.Visible;
                _mainPage.UncloseableMsgBox.Visibility = Visibility.Hidden;
                _mainPage.BtnBarcodeEnter.IsEnabled = true;
                _mainPage.BarcodeInput.IsEnabled = true;
                _mainPage.BarcodeInput.Text = "";
                _mainPage.BtnReport.IsEnabled = false;
                _mainPage.BtnReturn.Visibility = Visibility.Hidden;
                _mainPage.BtnTake.IsEnabled = false;
                _mainPage.BtnBack.Visibility = Visibility.Hidden;
                _mainPage.MainCanvasBlur.Radius = 0;
                _mainPage.BarcodeInput.Watermark = "Ввод ШК с клавиатуры";
                _mainPage.TroubleInputWindow.Visibility = Visibility.Hidden;
                _mainPage.MainStatus.Text = "Поднесите считыватель к штрих-коду или введите\r\nштрих - код с клавиатуры";

                MainFrame.NavigationService.Navigate(_mainPage);
            });
        }

        private List<Device> GetDevicesWithActualStateAndTasks()
        {
            var unsentOps = _operationRepository.GetAllUnsent();
            var unsentTasks = _taskRepository.GetAllUnsent();
            var devices = _deviceRepository.GetAll();
            foreach (var device in devices)
            {
                var unsentOpsforDevice = unsentOps.Where(o => o.DeviceId == device.Id).ToList();
                if (unsentOpsforDevice.Any())
                {
                    var lastUnsentOp = unsentOpsforDevice.OrderByDescending(o => o.CreateDate).First();
                    device.LastOperation = lastUnsentOp.Operation;
                    device.LastOperationUserId = lastUnsentOp.UserId;
                    device.LastOperationDate = lastUnsentOp.CreateDate;
                }
                device.OpenTasksCount += unsentTasks.Count(o => o.DeviceId == device.Id);
            }

            return devices;
        }

        private void OnMainMenuReturnTimer(Object source, ElapsedEventArgs e)
        {
            _mainMenuReturnTimer.Stop();

            _logger.Debug("user/device select timeout");

            StateMainMenu();
        }

        public void StateUserSelected()
        {
            SetState(AppState.UserSelected);

            CurrentDevice = null;
            CurrentOperation = null;

            _mainMenuReturnTimer.Start();

            Dispatcher.Invoke(() =>
            {
                _mainPage.TroubleInputWindow.Visibility = Visibility.Hidden;
                _mainPage.AdminLoginButton.Visibility = Visibility.Hidden;
                _mainPage.MsgBox.Visibility = Visibility.Hidden;
                _mainPage.UncloseableMsgBox.Visibility = Visibility.Hidden;
                _mainPage.BtnBarcodeEnter.IsEnabled = true;
                _mainPage.BarcodeInput.IsEnabled = true;
                _mainPage.BtnBack.Visibility = Visibility.Visible;
                _mainPage.BtnTake.IsEnabled = true;
                //_mainPage.BtnReturn.IsEnabled = true;
                _mainPage.BtnReport.IsEnabled = false;
                _mainPage.MainCanvasBlur.Radius = 0;
                _mainPage.BarcodeInput.Watermark = "Ввод ШК устройства";
                _mainPage.MainStatus.Text = "Нажмите 'взять устройство' или отсканируйте\r\nштрихкод устройства для возврата";

                MainFrame.NavigationService.Navigate(_mainPage);
            });
        }

        public void StateDeviceSelected()
        {
            SetState(AppState.DeviceSelected);

            CurrentUser = null;
            CurrentOperation = null;

            _mainMenuReturnTimer.Start();

            Dispatcher.Invoke(() =>
            {
                _mainPage.TroubleInputWindow.Visibility = Visibility.Hidden;
                _mainPage.AdminLoginButton.Visibility = Visibility.Hidden;
                _mainPage.MsgBox.Visibility = Visibility.Hidden;
                _mainPage.UncloseableMsgBox.Visibility = Visibility.Hidden;
                _mainPage.BtnBarcodeEnter.IsEnabled = true;
                _mainPage.BarcodeInput.IsEnabled = true;
                _mainPage.BtnBack.Visibility = Visibility.Visible;
                _mainPage.BtnTake.IsEnabled = false;
                //_mainPage.BtnReturn.IsEnabled = true;
                _mainPage.BtnReport.IsEnabled = false;
                _mainPage.MainCanvasBlur.Radius = 0;
                _mainPage.BarcodeInput.Watermark = "Ввод ШК сотрудника";
                _mainPage.MainStatus.Text = "Отсканируйте личный код";

                MainFrame.NavigationService.Navigate(_mainPage);
            });
        }

        public void StateDeviceReturnInProgress()
        {
            SetState(AppState.DeviceReturnInProgress);

            CurrentOperation = DeviceOperation.Surrender;

            Dispatcher.Invoke(() =>
            {
                _mainPage.TroubleInputWindow.Visibility = Visibility.Hidden;
                _mainPage.AdminLoginButton.Visibility = Visibility.Hidden;
                _mainPage.BtnBack.Visibility = Visibility.Hidden;
                _mainPage.MsgBox.Visibility = Visibility.Hidden;
                _mainPage.UncloseableMsgBox.Visibility = Visibility.Hidden;
                _mainPage.BtnBarcodeEnter.IsEnabled = false;
                _mainPage.BarcodeInput.IsEnabled = false;
                _mainPage.BtnTake.IsEnabled = false;
                //_mainPage.BtnReturn.IsEnabled = false;
                _mainPage.BtnReport.IsEnabled = true;
                _mainPage.MainCanvasBlur.Radius = 0;
                _mainPage.MainStatus.Text = "Закройте ячейку или сообщите о проблеме";

                MainFrame.NavigationService.Navigate(_mainPage);
            });
        }

        public void StateDeviceTakeInProgress()
        {
            SetState(AppState.DeviceTakeInProgress);

            CurrentOperation = DeviceOperation.Issue;

            Dispatcher.Invoke(() =>
            {
                _mainPage.TroubleInputWindow.Visibility = Visibility.Hidden;
                _mainPage.AdminLoginButton.Visibility = Visibility.Hidden;
                _mainPage.BtnBack.Visibility = Visibility.Hidden;
                _mainPage.MsgBox.Visibility = Visibility.Hidden;
                _mainPage.UncloseableMsgBox.Visibility = Visibility.Hidden;
                _mainPage.BtnBarcodeEnter.IsEnabled = false;
                _mainPage.BarcodeInput.IsEnabled = false;
                _mainPage.BtnTake.IsEnabled = false;
                //_mainPage.BtnReturn.IsEnabled = false;
                _mainPage.BtnReport.IsEnabled = true;
                _mainPage.MainCanvasBlur.Radius = 0;
                _mainPage.MainStatus.Text = "Закройте ячейку или сообщите о проблеме";

                MainFrame.NavigationService.Navigate(_mainPage);
            });
        }

        public void StateTroubleInput()
        {
            SetState(AppState.TroubleInput);

            Dispatcher.Invoke(() =>
            { 
                ReloadTroubleItems();

                _mainPage.TroubleInputWindow.Visibility = Visibility.Visible;
                _mainPage.MainCanvasBlur.Radius = 20;
                _mainPage.BtnSelect.IsEnabled = false;
                _mainPage.MsgBox.Visibility = Visibility.Hidden;
                _mainPage.UncloseableMsgBox.Visibility = Visibility.Hidden;

                MainFrame.NavigationService.Navigate(_mainPage);
            });
        }

        public void StateTroubleInputWaitCellClose()
        {
            SetState(AppState.TroubleInputWaitCellClose);
        }

        public void StateNoFreeDevices()
        {
            SetState(AppState.NoFreeDevices);

            StateMainError("нет свободных устройств");
        }

        public void StateAdminLogin()
        {
            SetState(AppState.AdminLogin);

            var adminLoginPage = new AdminLoginPage(this);

            adminLoginPage.AdminCanvasBlur.Radius = 0;
            adminLoginPage.MsgBox.Visibility = Visibility.Hidden;

            MainFrame.NavigationService.Navigate(adminLoginPage);
        }

        public void StateAdminList()
        {
            SetState(AppState.AdminList);

            if (_adminPage == null)
                _adminPage = new AdminPage(this);

            //var adminListPage = new AdminPage(this);

            _adminPage.CellWindow.Visibility = Visibility.Hidden;
            _adminPage.CellOpenReasonWindow.Visibility = Visibility.Hidden;
            _adminPage.ClearDbConfirmWindow.Visibility = Visibility.Hidden;
            _adminPage.UncloseableMsgBox.Visibility = Visibility.Hidden;
            _adminPage.ClearDbButton.Visibility = Visibility.Visible;
            _adminPage.AdminListCanvasBlur.Radius = 0;

            MainFrame.NavigationService.Navigate(_adminPage);
        }

        public void StateAdminCellDetail(CellDetail det)
        {
            SetState(AppState.AdminCellDetail);

            //var adminListPage = new AdminPage(this);
            Dispatcher.Invoke(() =>
            {
                _adminPage.CellName.Text = det.Name;
                _adminPage.CellModel.Text = det.Model;
                _adminPage.CellSerial.Text = det.Serial;
                _adminPage.CellPlace.Text = det.Place;
                _adminPage.CellCharge.Text = det.Charge;
                _adminPage.CellStatus.Text = det.StatusText;

                _adminPage.CellWindow.Visibility = Visibility.Visible;
                _adminPage.CellOpenReasonWindow.Visibility = Visibility.Hidden;
                _adminPage.ClearDbConfirmWindow.Visibility = Visibility.Hidden;
                _adminPage.UncloseableMsgBox.Visibility = Visibility.Hidden;
                _adminPage.AdminListCanvasBlur.Radius = 20;

                MainFrame.NavigationService.Navigate(_adminPage);
            });
        }

        public void StateAdminCellOpened(int cellId, bool canTake)
        {
            if (_currentState == AppState.AdminCellDetail)
            {
                SetState(AppState.AdminCellOpened);

                OpenedCell = cellId;
                _controller.OpenDoor(cellId);

                _adminPage.ReturnButton.IsEnabled = true;
                _adminPage.TakeButton.IsEnabled = canTake;

                _adminPage.CellWindow.Visibility = Visibility.Hidden;
                _adminPage.CellOpenReasonWindow.Visibility = Visibility.Visible;
                _adminPage.ClearDbConfirmWindow.Visibility = Visibility.Hidden;
                _adminPage.UncloseableMsgBox.Visibility = Visibility.Hidden;

                MainFrame.NavigationService.Navigate(_adminPage);
            }
        }

        public void StateAdminCellOpenReasonSelected()
        {
            if (_currentState == AppState.AdminCellOpened)
            {
                SetState(AppState.AdminCellOpenReasonSelected);

                _adminPage.ReturnButton.IsEnabled = false;
                _adminPage.TakeButton.IsEnabled = false;
                _adminPage.CellWindow.Visibility = Visibility.Hidden;
                _adminPage.CellOpenReasonWindow.Visibility = Visibility.Visible;
                _adminPage.ClearDbConfirmWindow.Visibility = Visibility.Hidden;
                _adminPage.UncloseableMsgBox.Visibility = Visibility.Hidden;

                MainFrame.NavigationService.Navigate(_adminPage);
            }
        }

        public void StateAdminClearDbConfirm()
        {
            if (_currentState == AppState.AdminList)
            {
                SetState(AppState.AdminClearDbConfirm);

                _adminPage.CellWindow.Visibility = Visibility.Hidden;
                _adminPage.CellOpenReasonWindow.Visibility = Visibility.Hidden;
                _adminPage.ClearDbConfirmWindow.Visibility = Visibility.Visible;
                _adminPage.UncloseableMsgBox.Visibility = Visibility.Hidden;
                _adminPage.AdminListCanvasBlur.Radius = 20;
            }
        }

        public void StateAdminClearDbStart()
        {
            if (_currentState == AppState.AdminClearDbConfirm)
            {
                SetState(AppState.AdminClearDbStart);

                _adminPage.CellWindow.Visibility = Visibility.Hidden;
                _adminPage.CellOpenReasonWindow.Visibility = Visibility.Hidden;
                _adminPage.ClearDbConfirmWindow.Visibility = Visibility.Hidden;
                _adminPage.UncloseableMsgBox.Visibility = Visibility.Visible;
                _adminPage.AdminListCanvasBlur.Radius = 20;
                //button ok hidden
                _adminPage.ButtonOkUMsgBox.Visibility = Visibility.Hidden;
                _adminPage.UMsgBoxTitle.Text = "Ожидание";
                _adminPage.UMsgBoxMessage.Text = "Идет процесс очистки БД";

                DbSyncronizer.GetInstance().ClearDatabase();

                StateAdminClearDbFinished();
            }
        }

        public void StateAdminClearDbFinished()
        {
            if (_currentState == AppState.AdminClearDbStart)
            {
                SetState(AppState.AdminClearDbFinished);

                _adminPage.CellWindow.Visibility = Visibility.Hidden;
                _adminPage.CellOpenReasonWindow.Visibility = Visibility.Hidden;
                _adminPage.ClearDbConfirmWindow.Visibility = Visibility.Hidden;
                _adminPage.UncloseableMsgBox.Visibility = Visibility.Visible;
                _adminPage.AdminListCanvasBlur.Radius = 20;
                //button ok visible
                _adminPage.ButtonOkUMsgBox.Visibility = Visibility.Visible;
                _adminPage.UMsgBoxTitle.Text = "Успешно";
                _adminPage.UMsgBoxMessage.Text = "Очистка БД завершена";
            }
        }

        //event handlers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StateInit();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Exit();
        }

        private void Exit()
        {
            _logger.Info("application exit");
            _controller?.CloseDevice();
            Application.Current.Shutdown();
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var code = _serialPort.ReadExisting().Trim();
            //Debug.WriteLine("scanned code: " + code);
            BarcodeProcess(code);
        }

        public void BarcodeProcess(string barcode)
        {
            _logger.Debug($"BarcodeProcess: [{barcode}]");
            if (_currentState == AppState.MainMenu)
            {
                //надо определить штрихкод чего отсканили
                var users = _userRepository.GetUsersByBarcode(barcode);
                if (users.Count > 1)
                {
                    StateMainError("найдено более одного пользователя");
                }
                else if (users.Count == 1)
                {
                    //user found
                    CurrentUser = users.First();

                    StateUserSelected();
                }
                else
                {
                    var devices = _deviceRepository.GetDevicesByBarcode(barcode);
                    switch (devices.Count)
                    {
                        case 1:
                            //one device found
                            CurrentDevice = devices.First();

                            StateDeviceSelected();
                            break;
                        case 0:
                            //error window
                            StateMainError("неизвестный штрихкод");
                            break;
                        default:
                            StateMainError("найдено более одного устройства");
                            break;
                    }
                }
            }
            else if (_currentState == AppState.UserSelected)
            {
                _mainMenuReturnTimer.Stop();
                //need device barcode
                var devices = _deviceRepository.GetDevicesByBarcode(barcode);
                switch (devices.Count)
                {
                    case 1:
                        //one device found
                        CurrentDevice = devices.First();

                        var cellNum = CurrentDevice.CellNumber;
                        OpenedCell = cellNum;
                        _controller.OpenDoor(cellNum);

                        StateDeviceReturnInProgress();
                        break;
                    case 0:
                        //error window
                        StateMainError("неизвестный штрихкод устройства");
                        break;
                    default:
                        StateMainError("найдено более одного устройства");
                        break;
                }
            }
            else if (_currentState == AppState.DeviceSelected)
            {
                _mainMenuReturnTimer.Stop();

                var users = _userRepository.GetUsersByBarcode(barcode);
                switch (users.Count)
                {
                    case 1:
                        //user found
                        CurrentUser = users.First();

                        var cellNum = CurrentDevice.CellNumber;
                        OpenedCell = cellNum;
                        _controller.OpenDoor(cellNum);

                        StateDeviceReturnInProgress();
                        break;

                    case 0:
                        //error window
                        StateMainError("неизвестный штрихкод сотрудника");
                        break;

                    default:
                        StateMainError("найдено более одного пользователя");
                        break;
                }
            }
        }

        public void BtnTake_Click()
        {
            if (_currentState == AppState.UserSelected)
            {
                //detect free device
                var freeDevices = GetDevicesWithActualStateAndTasks().Where(o => o.LastCharge == 100 && o.OpenTasksCount == 0 && o.LastOperation == DeviceOperation.Surrender).ToList();

                if (freeDevices.Any())
                {
                    //give
                    CurrentDevice = freeDevices.First();

                    int cellNum = CurrentDevice.CellNumber;
                    OpenedCell = cellNum;
                    _controller.OpenDoor(cellNum);

                    StateDeviceTakeInProgress();
                }
                else
                {
                    //error
                    StateNoFreeDevices();
                }
            }
        }

        //public void BtnReturn_Click()
        //{
        //    if (_currentState == AppState.UserSelected)
        //    {
        //        var userDevices = GetDevicesWithActualState().Where(o =>
        //                o.LastOperation == DeviceOperation.Surrender && o.LastOperationUserId == CurrentUser.Id)
        //            .Select(o => new ListBoxItem
        //            {
        //                Name = "item_" + o.Id,
        //                Content = $"{o.Serial} ячейка {o.CellNumber}"
        //            }).ToList();



        //        if (_userBarcode != null && _userBarcodes.ContainsKey(_userBarcode))
        //        {
        //            int cellId = _userBarcodes[_userBarcode];
        //            OpenedCell = cellId;
        //            _controller.OpenDoor(cellId);

        //            StateDeviceOperInProgress();
        //        }
        //        else
        //        {
        //            var code = _mainPage.BarcodeInput.Text.Trim();
        //            BarcodeProcess(code);
        //        }
        //    }
        //}

        public void BtnReport_Click()
        {
            if (_currentState == AppState.DeviceReturnInProgress || _currentState == AppState.DeviceTakeInProgress)
            {
                StateTroubleInput();
            }
        }

        public void BtnTroubleInputSelect_Click(int? troubleId)
        {
            if (!troubleId.HasValue)
            {
                ShowTroubleMessage("инцидент", "ошибка");
                return;
            }

            try
            {
                _taskRepository.Add(new TaskOut
                {
                    CreateDate = DateTime.Now,
                    DeviceFaultId = troubleId.Value,
                    UserId = CurrentUser.Id,
                    DeviceId = CurrentDevice.Id
                });

                if (OpenedCell == null)
                {
                    ShowTroubleMessage("инцидент", "создан успешно");
                }
                else
                {
                    //uncloseabvle window
                    StateTroubleInputWaitCellClose();
                    ShowUncloseableMessage("инцидент", "создан успешно, положите назад устройство и закройте ячейку");
                }
            }
            catch (Exception)
            {
                //log?
                ShowTroubleMessage("инцидент", "ошибка БД");
            }
        }

        public void TroubleItem_Checked(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _mainPage.BtnSelect.IsEnabled = true;
            });
        }

        private void ShowTroubleMessage(string title, string msg)
        {
            Dispatcher.Invoke(() =>
            {
                _mainPage.MainCanvasBlur.Radius = 20;
                _mainPage.MsgBoxTitle.Text = title;
                _mainPage.MsgBoxMessage.Text = msg;
                _mainPage.TroubleInputWindow.Visibility = Visibility.Hidden;
                _mainPage.MsgBox.Visibility = Visibility.Visible;
            });
        }

        private void ShowUncloseableMessage(string title, string msg)
        {
            Dispatcher.Invoke(() =>
            {
                _mainPage.MainCanvasBlur.Radius = 20;
                _mainPage.UMsgBoxTitle.Text = title;
                _mainPage.UMsgBoxMessage.Text = msg;
                _mainPage.TroubleInputWindow.Visibility = Visibility.Hidden;
                _mainPage.MsgBox.Visibility = Visibility.Hidden;
                _mainPage.UncloseableMsgBox.Visibility = Visibility.Visible;
            });
        }

        private void ReloadTroubleItems()
        {
            //            < RadioButton x: Name = "Trouble1" Style = "{DynamicResource TroubleButtonStyle}"  Height = "50" Width = "658" FontSize = "30.00" FontFamily = "Calibri Light" Checked = "Trouble1_Checked" > Разбит экран 1 </ RadioButton >
            var list = _deviceFaultRepository.GetAll();

            var items = new List<RadioButton>();
            foreach (var item in list)
            {
                var radio = new RadioButton
                {
                    Name = $"Trouble_{item.Id}",
                    Style = FindResource("TroubleButtonStyle") as Style,
                    Height = 50,
                    Width = 658,
                    FontSize = 30,
                    FontFamily = new FontFamily("Calibri Light"),
                    Content = item.Name
                };
                radio.Checked += TroubleItem_Checked;

                items.Add(radio);
            }

            Dispatcher.Invoke(() =>
            {
                _mainPage.TroubleSvp.Children.Clear();
                foreach (var item in items)
                {
                    _mainPage.TroubleSvp.Children.Add(item);
                }
            });
        }

        public void BtnMainMsgBoxReturn_Click()
        {
            switch (_currentState)
            {
                case AppState.InitError:
                    Exit();
                    break;

                case AppState.MainError:
                    switch (_prevState)
                    {
                        case AppState.MainMenu:
                            StateMainMenu();
                            break;

                        case AppState.UserSelected:
                            StateUserSelected();
                            break;

                        case AppState.DeviceSelected:
                            StateDeviceSelected();
                            break;

                        case AppState.NoFreeDevices:
                            StateMainMenu();
                            break;

                        default:
                            StateMainMenu();
                            break;
                    }
                    break;

                case AppState.TroubleInput:
                    StateMainMenu();
                    break;
            }
        }

        public void TroubleInputCancel_Click()
        {
            if (_currentState == AppState.TroubleInput)
            {
                switch (_prevState)
                {
                    case AppState.DeviceReturnInProgress:
                        StateDeviceReturnInProgress();
                        break;

                    case AppState.DeviceTakeInProgress:
                        StateDeviceTakeInProgress();
                        break;

                    default:
                        StateMainMenu();
                        break;
                }
            }
        }

        public void BtnBack_Click()
        {
            _mainMenuReturnTimer.Stop();

            switch (_currentState)
            {
                case AppState.UserSelected:
                    StateMainMenu();
                    break;

                case AppState.DeviceSelected:
                    StateMainMenu();
                    break;

                case AppState.AdminLogin:
                    StateMainMenu();
                    break;

                case AppState.AdminList:
                    StateMainMenu();
                    break;
            }
        }

        public void BtnAdmin_Click()
        {
            _mainMenuReturnTimer.Stop();

            switch (_currentState)
            {
                case AppState.MainMenu:
                    StateAdminLogin();
                    break;
            }
        }

        private void ControllerOnSensorStateChangedEvent(int sensorType, int sensorNumber, bool value)
        {
            _logger.Debug($"ControllerOnSensorStateChangedEvent sensorType={sensorType}, sensorNumber={sensorNumber}, value={value}");
            _dicCells[sensorNumber] = value;
            if (value == false) //close
            {
                if (OpenedCell != null && sensorNumber == OpenedCell)
                {
                    OpenedCell = null;
                    switch (_currentState)
                    {
                        case AppState.DeviceReturnInProgress:
                        case AppState.DeviceTakeInProgress:
                            {
                                if (CurrentOperation.HasValue)
                                {
                                    _operationRepository.Add(new OperationOut
                                    {
                                        CreateDate = DateTime.Now,
                                        DeviceId = CurrentDevice.Id,
                                        UserId = CurrentUser.Id,
                                        Operation = CurrentOperation.Value
                                    });
                                }

                                StateMainMenu();
                                break;
                            }

                        case AppState.TroubleInputWaitCellClose:
                            {
                                //записывать операцию только если возврат, выдача с проблемой - значит нет выдачи (оставили в ячейке)
                                if (CurrentOperation.HasValue && CurrentOperation == DeviceOperation.Surrender)
                                {
                                    _operationRepository.Add(new OperationOut
                                    {
                                        CreateDate = DateTime.Now,
                                        DeviceId = CurrentDevice.Id,
                                        UserId = CurrentUser.Id,
                                        Operation = CurrentOperation.Value
                                    });
                                }

                                StateMainMenu();
                                break;
                            }

                        case AppState.TroubleInput:
                            {
                                _logger.Debug($"door closed in state [{_currentState}], auto-reopen");
                                OpenedCell = sensorNumber;
                                _controller.OpenDoor(sensorNumber);
                                break;
                            }


                        case AppState.AdminCellOpened:
                            //закрыли не выбрав причину - переоткрыть
                            {
                                _logger.Debug($"door closed in state [{_currentState}], auto-reopen");
                                OpenedCell = sensorNumber;
                                _controller.OpenDoor(sensorNumber);
                                break;
                            }

                        case AppState.AdminCellOpenReasonSelected:
                            {
                                var det = _adminPage.GetSelectedCellDetail();
                                if (det != null)
                                {
                                    StateAdminCellDetail(det);
                                }

                                break;
                            }
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////
        //init
        private void InitBarcodeScanner()
        {
            if (_barcodeScannerPort.Length > 0)
            {
                _logger.Info("barcode scanner init");
                _serialPort = new SerialPort(_barcodeScannerPort, 115200, Parity.None, 8, StopBits.One)
                {
                    Handshake = Handshake.None,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };
                _serialPort.DataReceived += SerialPortDataReceived;
                _serialPort.Open();
            }
        }

        private void InitCellsController()
        {
            if (_cellControllerIp == null || _cellControllerPort == 0)
            {
                _logger.Info("cells controller init: DummyCellsController");
                _controller = new DummyCellsController
                {
                    IPAddress = _cellControllerIp,
                    Port = _cellControllerPort
                };
            }
            else
            {
                _logger.Info("cells controller init: CellsController");
                _controller = new CellsController
                {
                    IPAddress = _cellControllerIp,
                    Port = _cellControllerPort
                };
            }

            var cells = new Dictionary<int, ControllerCellInfo>()
            {
                {1, new ControllerCellInfo{ControllerIdx = 2, CellIdx = 13}},
                {2, new ControllerCellInfo{ControllerIdx = 2, CellIdx = 6}},
                {3, new ControllerCellInfo{ControllerIdx = 2, CellIdx = 7}},
                {4, new ControllerCellInfo{ControllerIdx = 2, CellIdx = 10}},
                {5, new ControllerCellInfo{ControllerIdx = 2, CellIdx = 12}},
                {6, new ControllerCellInfo{ControllerIdx = 2, CellIdx = 14}},
                {7, new ControllerCellInfo{ControllerIdx = 2, CellIdx = 8}},
                {8, new ControllerCellInfo{ControllerIdx = 2, CellIdx = 15}},
                {9, new ControllerCellInfo{ControllerIdx = 2, CellIdx = 11}},
                {10, new ControllerCellInfo{ControllerIdx = 2, CellIdx = 9}},
                {11, new ControllerCellInfo{ControllerIdx = 0, CellIdx = 9}},
                {12, new ControllerCellInfo{ControllerIdx = 0, CellIdx = 14}},
                {13, new ControllerCellInfo{ControllerIdx = 0, CellIdx = 10}},
                {14, new ControllerCellInfo{ControllerIdx = 0, CellIdx = 13}},
                {15, new ControllerCellInfo{ControllerIdx = 0, CellIdx = 12}},
                {16, new ControllerCellInfo{ControllerIdx = 0, CellIdx = 8}},
                {17, new ControllerCellInfo{ControllerIdx = 0, CellIdx = 11}},
                {18, new ControllerCellInfo{ControllerIdx = 1, CellIdx = 11}},
                {19, new ControllerCellInfo{ControllerIdx = 1, CellIdx = 8}},
                {20, new ControllerCellInfo{ControllerIdx = 1, CellIdx = 9}},
                {21, new ControllerCellInfo{ControllerIdx = 1, CellIdx = 10}},
                {22, new ControllerCellInfo{ControllerIdx = 1, CellIdx = 15}},
                {23, new ControllerCellInfo{ControllerIdx = 1, CellIdx = 14}},
                {24, new ControllerCellInfo{ControllerIdx = 1, CellIdx = 12}},
                {25, new ControllerCellInfo{ControllerIdx = 1, CellIdx = 5}},
                {26, new ControllerCellInfo{ControllerIdx = 1, CellIdx = 7}},
                {27, new ControllerCellInfo{ControllerIdx = 1, CellIdx = 13}},
            };
            const int maxControllerIdx = 2;
            const int maxCellIdx = 15;
            int[,] cellsMap = new int[maxControllerIdx + 1, maxCellIdx + 1];
            foreach (var pair in cells)
            {
                cellsMap[pair.Value.ControllerIdx, pair.Value.CellIdx] = pair.Key;
            }

            if (!_controller.OpenDevice())
            {
                _logger.Error("controller init error: OpenDevice failed");
                throw new Exception("Ошибка инициализации контроллера.");
            }

            if (!_controller.InitCellsMap(cellsMap))
            {
                _logger.Error("controller init error: InitCellsMap failed");
                throw new Exception("Ошибка инициализации контроллера.");
            }

            _controller.SensorStateChangedEvent += ControllerOnSensorStateChangedEvent;
            _controller.BeginScanSensors();
            foreach (var item in _controller.DoorSensorsState)
            {
                _dicCells.Add(item.Key, item.Value != null && item.Value.Value);
            }
        }

        private void InitDbSyncTimer()
        {
            _logger.Info($"init db sync timer with interval: [{_dbSyncInterval}] sec.");
            var dbSyncTimer = new Timer();
            dbSyncTimer.Elapsed += OnDbSyncTimer;
            dbSyncTimer.Interval = _dbSyncInterval * 1000;
            dbSyncTimer.Enabled = true;
        }

        private static readonly object TimerDbLocker = new object();

        private static void OnDbSyncTimer(object source, ElapsedEventArgs e)
        {
            //для исключения одновременных запусков обработки, если предыдущие не завершились
            if (Monitor.TryEnter(TimerDbLocker))
            {
                try
                {
                    //попытка синхронизации баз данных, если неудачно то ожидаем следующей
                    DbSyncronizer.GetInstance().SyncDatabases();
                    //Debug.WriteLine("sync success");
                }
                catch (Exception)
                {
                    //log?
                    //Debug.WriteLine("sync error");
                }
                finally
                {
                    Monitor.Exit(TimerDbLocker);
                }
            }
        }
    }
}
