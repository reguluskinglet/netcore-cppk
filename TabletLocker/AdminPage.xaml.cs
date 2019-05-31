using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NLog;
using Rzdppk.Model.Enums;
using TabletLocker.Db;
using TabletLocker.Db.Interfaces;
using TabletLocker.Model;

namespace TabletLocker
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        private readonly PageContainer _container;
        private readonly ITaskRepository _taskRepository = new TaskRepository();
        private readonly IDeviceRepository _deviceRepository = new DeviceRepository();
        private readonly IUserRepository _userRepository = new UserRepository();
        private readonly IOperationRepository _operationRepository = new OperationRepository();
        private int? _selectedCell;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public AdminPage(PageContainer cont)
        {
            InitializeComponent();

            _container = cont;

            var timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                TimeMark.Text = DateTime.Now.ToString("HH:mm");
                DateMark.Text = DateTime.Now.ToString("dd.MM.yyyy");
            }, Dispatcher);

            CellsGrid.ItemsSource = LoadTableData();

            var timerTable = new DispatcherTimer(new TimeSpan(0, 0, 30), DispatcherPriority.Normal, delegate
            {
                //MainTable.ItemsSource = LoadTableData();
                CellsGrid.ItemsSource = LoadTableData();
            }, Dispatcher);
            timerTable.Start();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            _container.BtnBack_Click();
        }

        //private void Grid_Click(object sender, MouseButtonEventArgs e)
        //{
        //    if (sender != null && sender is DataGrid grid && grid.SelectedItems.Count == 1)
        //    {
        //        var dgr = grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem) as DataGridRow;
        //        if (dgr?.Item is CellStatusItem obj)
        //        {
        //            var id = obj.Number;
        //            //Debug.WriteLine($"clicked cell num: {id}");
        //        }
        //    }
        //}

        private string GetDeviceStatus(Device dev)
        {
            var status = "-";

            var taskCount = dev.OpenTasksCount;
            switch (dev.LastOperation)
            {
                case DeviceOperation.Surrender:
                    status = "в наличии";
                    break;
                case DeviceOperation.Issue:
                    status = "отсутствует";
                    break;
            }

            if (status != "-")
            {
                status += taskCount > 0 ? " (Р)" : " (И)";
            }

            return status;
        }

        private string GetRepairStatus(Device dev)
        {
            var status = "-";

            var taskCount = dev.OpenTasksCount;

            if (dev.LastOperation.HasValue)
            {
                status = taskCount > 0 ? "в ремонте" : "исправен";
            }

            return status;
        }

        private List<CellStatusItem> LoadTableData()
        {
            var ret = new List<CellStatusItem>();

            var unsentTasks = _taskRepository.GetAllUnsent();
            var unsentOpers = _operationRepository.GetAllUnsent();
            var devices = _deviceRepository.GetAll().ToDictionary(o => o.CellNumber);
            for (var i = 1; i <= 27; i++)
            {
                var item = new CellStatusItem
                {
                    Number = i,
                    Name = $"Ячейка {i}"
                };
                if (devices.ContainsKey(i))
                {
                    var dev = devices[i];
                    var unsentTasksForDevice = unsentTasks.Where(o => o.DeviceId == dev.Id);
                    var unsentOperForDevice = unsentOpers.Where(o => o.DeviceId == dev.Id).OrderBy(o => o.CreateDate)
                        .FirstOrDefault();
                    if (unsentOperForDevice != null)
                    {
                        dev.LastOperation = unsentOperForDevice.Operation;
                        dev.LastOperationUserId = unsentOperForDevice.UserId;
                        dev.LastOperationDate = unsentOperForDevice.CreateDate;
                    }

                    dev.OpenTasksCount += unsentTasksForDevice.Count();

                    item.State = GetDeviceStatus(dev);
                    item.Charge = dev.LastCharge == null ? "-" : $"заряжен на {dev.LastCharge}%";
                }
                else
                {
                    item.State = "-";
                    item.Charge = "-";
                }
                ret.Add(item);
            }

            return ret;
        }

        private void DataGrid_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedIndex != -1)
            {
                var dgr = (DataGridRow) dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
                if (dgr?.Item is CellStatusItem obj)
                {
                    var id = obj.Number;
                    _logger.Debug($"selected cell num: {id}");

                    var det = GetCellDetail(id);

                    if (det != null)
                    {
                        _selectedCell = id;
                        _container.StateAdminCellDetail(det);
                    }
                }
            }
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCell != null)
            {
                var dev = GetDeviceByCellNumber(_selectedCell.Value);
                _container.StateAdminCellOpened(_selectedCell.Value, dev?.LastOperation == DeviceOperation.Surrender);
            }
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCell != null)
            {
                var dev = GetDeviceByCellNumber(_selectedCell.Value);

                if (dev != null && _container.CurrentUser != null)
                {
                    //возврат устройства, если оно было в ремонте
                    _operationRepository.Add(new OperationOut
                    {
                        DeviceId = dev.Id,
                        UserId = _container.CurrentUser.Id,
                        Operation = DeviceOperation.Surrender,
                        CreateDate = DateTime.Now
                    });

                    _container.StateAdminCellOpenReasonSelected();
                }
                else
                {
                    //error?
                }
 
            }
            else
            {
                //error?
            }
        }

        private void BtnTake_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCell != null)
            {
                var dev = _deviceRepository.GetDeviceByCellNumber(_selectedCell.Value);

                if (dev != null && _container.CurrentUser != null)
                {
                    _operationRepository.Add(new OperationOut
                    {
                        DeviceId = dev.Id,
                        UserId = _container.CurrentUser.Id,
                        Operation = DeviceOperation.Issue,
                        CreateDate = DateTime.Now
                    });

                    _container.StateAdminCellOpenReasonSelected();
                }
                else
                {
                    //error
                    //_container.StateAdminList();
                }
            }
            else
            {
                //error
                //_container.StateAdminList();
            }
        }

        private void BtnCellDetailClose_Click(object sender, RoutedEventArgs e)
        {
            _container.StateAdminList();
        }

        public CellDetail GetSelectedCellDetail()
        {
            if (_selectedCell != null)
            {
                return GetCellDetail(_selectedCell.Value);
            }

            return null;
        }

        private Device GetDeviceByCellNumber(int num)
        {
            var dev = _deviceRepository.GetDeviceByCellNumber(num);

            if (dev != null)
            {
                var lastUnsentOperForDevice = _operationRepository.GetLastUnsentForDevice(dev.Id);
                if (lastUnsentOperForDevice != null)
                {
                    dev.LastOperation = lastUnsentOperForDevice.Operation;
                    dev.LastOperationUserId = lastUnsentOperForDevice.UserId;
                    dev.LastOperationDate = lastUnsentOperForDevice.CreateDate;
                }

                var unsentTasksForDevice = _taskRepository.GetUnsentForDevice(dev.Id);
                dev.OpenTasksCount += unsentTasksForDevice.Count;
            }

            return dev;
        }

        private CellDetail GetCellDetail(int id)
        {
            CellDetail ret = null;

            var dev = GetDeviceByCellNumber(id);

            if (dev != null)
            {
                ret = new CellDetail
                {
                    Name = $"Ячейка {id}",
                    StatusText = GetRepairStatus(dev),
                    Model = dev.Name,
                    Serial = dev.Serial
                };

                if (dev.LastCharge != null)
                {
                    ret.Charge = $"{dev.LastCharge}%";
                }

                if (dev.LastOperation == DeviceOperation.Surrender)
                {
                    ret.Place = "в ячейке";
                }
                else
                {
                    if (dev.LastOperationUserId == null)
                    {
                        ret.Place = "-";
                    }
                    else
                    { 
                        var user = _userRepository.GetUserById(dev.LastOperationUserId.Value);
                        ret.Place = UserRepository.GetShortFio(user?.Name);
                    }
                }
            }

            return ret;
        }

        private void ClearDbButton_OnClick(object sender, RoutedEventArgs e)
        {
            //state confirm
            _container.StateAdminClearDbConfirm();
        }

        private void BtnOkConfirm_OnClick(object sender, RoutedEventArgs e)
        {
            //state wait
            _container.StateAdminClearDbStart();
        }

        private void BtnCancelConfirm_OnClick(object sender, RoutedEventArgs e)
        {
            //state main table
            _container.StateAdminList();
        }

        private void ButtonOkUncloseableMsgBox_OnClick(object sender, RoutedEventArgs e)
        {
            //state main table
            _container.StateAdminList();
        }
    }

    public class CellStatusItem
    {
        public int Number { get; set; }

        public string Name { get; set; }

        public string State { get; set; }

        public string Charge { get; set; }
    }
}
