using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog;

namespace TabletLocker.CellController
{
    public class CellsController : ICellsController
    {
        private readonly byte[] _masks = new byte[8]
        {
            (byte) 1,
            (byte) 2,
            (byte) 4,
            (byte) 8,
            (byte) 16,
            (byte) 32,
            (byte) 64,
            (byte) 128
        };
        private Thread _thread = (Thread)null;
        private TcpClient _client = (TcpClient)null;
        private bool _scanProcess = false;
        private int[,] _cells = (int[,])null;
        private Dictionary<byte, Dictionary<byte, bool[]>> _controllerCellsLights = (Dictionary<byte, Dictionary<byte, bool[]>>)null;
        private Dictionary<byte, CellsControllerInfo> _controllers = new Dictionary<byte, CellsControllerInfo>();
        private Dictionary<int, bool?> _cellSensorsState = new Dictionary<int, bool?>();
        private Dictionary<int, bool?> _doorSensorsState = new Dictionary<int, bool?>();

        public Dictionary<byte, CellsControllerInfo> Controllers => _controllers;

        public Dictionary<int, bool?> DoorSensorsState => _doorSensorsState;

        public Dictionary<int, bool?> CellSensorsState => _cellSensorsState;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public string DeviceName => "CellsController";

        public string DeviceDriverClassName => nameof(CellsController);

        public string DeviceSerialNumber => "";

        public string DeviceDescription => "";

        public bool OpenDevice()
        {
            try
            {
                lock ("CellsControllerRequestLock")
                {
                    CloseDevice();
                    _client = new TcpClient();
                    _client.Connect(new IPEndPoint(System.Net.IPAddress.Parse(IPAddress), Port));
                    if (_client.Connected)
                    {
                        return true;
                    }
                    _logger.Error("Не удалось установить связь с контроллером.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }

        public void CloseDevice()
        {
            try
            {
                lock ("CellsControllerRequestLock")
                {
                    EndScanSensors();
                    if (_client != null && _client.Connected)
                        _client.Close();
                    _client = (TcpClient)null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public string IPAddress { get; set; }

        public int Port { get; set; }

        public int DeviceNumber
        {
            get
            {
                try
                {
                    //if (this._Settings.ContainsKey(nameof(DeviceNumber)))
                    //    return int.Parse(this._Settings[nameof(DeviceNumber)]);
                    return 0;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    return 0;
                }
            }
        }

        public event SensorStateChangedEventHandler SensorStateChangedEvent;

        public bool BeginScanSensors()
        {
            try
            {
                EndScanSensors();
                _thread = new Thread(BeginScanSensorsThread) { IsBackground = true };
                _thread.Start();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }

        private void BeginScanSensorsThread()
        {
            try
            {
                _scanProcess = true;
                while (_scanProcess)
                {
                    if (_controllers.Count > 0)
                    {
                        foreach (byte key in _controllers.Keys)
                        {
                            ScanControllerCells(key, true);
                            if (_scanProcess)
                                Thread.Sleep(10);
                            else
                                break;
                        }
                    }
                    else
                    {
                        if (!_scanProcess)
                            break;
                        Thread.Sleep(100);
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                _logger.Error(ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public void EndScanSensors()
        {
            try
            {
                _scanProcess = false;
                if (_thread == null || _thread.Join(2000))
                    return;
                _thread.Abort();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                _thread = (Thread)null;
            }
        }

        public bool InitCellsMap(int[,] cellsMap)
        {
            try
            {
                EndScanSensors();
                _cells = cellsMap;
                _controllers = new Dictionary<byte, CellsControllerInfo>();
                _doorSensorsState = new Dictionary<int, bool?>();
                _cellSensorsState = new Dictionary<int, bool?>();
                for (int index1 = 0; index1 <= _cells.GetUpperBound(0); ++index1)
                {
                    for (int index2 = 0; index2 <= _cells.GetUpperBound(1); ++index2)
                    {
                        if (_cells[index1, index2] > 0)
                        {
                            _doorSensorsState.Add(_cells[index1, index2], new bool?());
                            _cellSensorsState.Add(_cells[index1, index2], new bool?());
                            if (!_controllers.ContainsKey((byte)index1))
                            {
                                var info = GetControllerInfo((byte)index1);
                                if (info == null)
                                {
                                    throw new Exception($"ошибка получения данных о контроллере [{index1}]");
                                }
                                _controllers.Add((byte)index1, info);
                            }
                        }
                    }
                }
                foreach (byte key in _controllers.Keys)
                    ScanControllerCells(key, false);
                //logger.Debug("initial cell map: "+string.Join(";",_cellSensorsState.Select(o => $"{o.Key}: {o.Value}")));
                //logger.Debug("initial door map: " + string.Join(";", _doorSensorsState.Select(o => $"{o.Key}: {o.Value}")));
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }

        private void ScanControllerCells(byte controllerNumber, bool callChangeStateEvent)
        {
            //logger.Debug($"ScanControllerCells, num={ControllerNumber}");
            try
            {
                byte[] request = new byte[5]
                {
                    (byte) 2,
                    (byte) ((uint) controllerNumber * 16U),
                    (byte) 48,
                    (byte) 3,
                    (byte) 0
                };
                request[4] = (byte)((uint)request[0] + (uint)request[1] + (uint)request[2] + (uint)request[3]);
                byte[] numArray1 = this.SendCommand(request, true);
                if (numArray1 == null || numArray1.Length != 9)
                    return;
                byte[] numArray2 = new byte[2];
                byte[] numArray3 = new byte[2];
                for (int index = 0; index < 2; ++index)
                {
                    numArray3[index] = numArray1[index + 3];
                    numArray2[index] = numArray1[index + 5];
                }
                for (byte index1 = 0; index1 < (byte)2; ++index1)
                {
                    for (byte index2 = 0; index2 < (byte)8; ++index2)
                    {
                        //logger.Debug($"scan controller {ControllerNumber}, index1 {index1}, index2 {index2}");
                        byte num = (byte)((uint)index1 * 8U + (uint)index2 + (uint)_controllers[controllerNumber].FirstCellIndex);
                        //logger.Debug($"cell num={num}");
                        if (num <= _cells.GetUpperBound(1) && _cells[controllerNumber, num] > 0)
                        {

                            bool flag1 = (numArray2[index1] & _masks[index2]) > 0;
                            bool? nullable = _cellSensorsState[_cells[controllerNumber, num]];
                            bool flag2 = flag1;
                            if (nullable.GetValueOrDefault() != flag2 || !nullable.HasValue)
                            {
                                _cellSensorsState[_cells[controllerNumber, num]] = flag1;
                                // ISSUE: reference to a compiler-generated field
                                if (SensorStateChangedEvent != null & callChangeStateEvent)
                                {
                                    // ISSUE: reference to a compiler-generated field
                                    SensorStateChangedEvent(1, _cells[controllerNumber, num], flag1);
                                }
                            }

                            //logger.Debug($"cell exist, reading door bit {index2} [{_Masks[index2]}] of byte {index1} [{numArray3[index1]}], result = {(numArray3[index1] & _Masks[index2])}");
                            bool flag3 = (numArray3[index1] & _masks[index2]) == 0;
                            //logger.Debug($"door status = {flag3}");
                            nullable = _doorSensorsState[_cells[controllerNumber, num]];
                            //logger.Debug($"old door status = {nullable}");
                            bool flag4 = flag3;
                            if (nullable.GetValueOrDefault() != flag4 || !nullable.HasValue)
                            {
                                //logger.Debug($"need update door status");
                                _doorSensorsState[_cells[controllerNumber, num]] = flag3;
                                //logger.Debug($"set _doorSensorsState[{_cells[ControllerNumber, num]}] = {flag3}");
                                // ISSUE: reference to a compiler-generated field
                                if (SensorStateChangedEvent != null & callChangeStateEvent)
                                {
                                    // ISSUE: reference to a compiler-generated field
                                    SensorStateChangedEvent(2, _cells[controllerNumber, num], flag3);
                                }
                            }
                        }
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                _logger.Error(ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            //logger.Debug($"END ScanControllerCells");
        }

        public bool OpenDoor(int cellNumber)
        {
            _logger.Debug($"OpenDoor CellNumber: [{cellNumber}]");
            try
            {
                bool flag = false;
                for (int index1 = 0; index1 <= _cells.GetUpperBound(0); ++index1)
                {
                    for (int index2 = 0; index2 <= _cells.GetUpperBound(1); ++index2)
                    {
                        if (_cells[index1, index2] == cellNumber)
                        {
                            flag = true;
                            byte[] Request = new byte[5]
                            {
                                (byte) 2,
                                (byte) (index1 * 16 + index2),
                                (byte) 49,
                                (byte) 3,
                                (byte) 0
                            };
                            Request[4] = (byte)((uint)Request[0] + (uint)Request[1] + (uint)Request[2] + (uint)Request[3]);
                            SendCommand(Request, false);
                            Thread.Sleep(500);
                        }
                    }
                }
                if (!flag)
                    throw new Exception("Ячейка с номером " + cellNumber + " не найдена в конфигурации.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }

        public CellsControllerInfo GetControllerInfo(byte controllerNumber)
        {
            try
            {
                byte[] Request = new byte[5]
                {
                    (byte) 2,
                    (byte) ((uint) controllerNumber * 16U),
                    (byte) 48,
                    (byte) 3,
                    (byte) 0
                };
                Request[4] = (byte)((uint)Request[0] + (uint)Request[1] + (uint)Request[2] + (uint)Request[3]);
                byte[] numArray = SendCommand(Request, true);
                if (numArray == null || numArray.Length != 9)
                {
                    _logger.Debug($"GetControllerInfo {controllerNumber} - null");
                    return (CellsControllerInfo)null;
                }

                _logger.Debug($"GetControllerInfo {controllerNumber} - not null");
                return new CellsControllerInfo()
                {
                    CellsCount = 16,
                    FirstCellIndex = 0
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return (CellsControllerInfo)null;
            }
        }

        public bool SetAllLights()
        {
            try
            {
                bool flag = true;
                this._controllerCellsLights = new Dictionary<byte, Dictionary<byte, bool[]>>();
                foreach (byte key in _controllers.Keys)
                {
                    byte[] Request = new byte[11]
                    {
                        (byte) 2,
                        (byte) ((int) key * 16 + 4),
                        (byte) 50,
                        byte.MaxValue,
                        byte.MaxValue,
                        byte.MaxValue,
                        byte.MaxValue,
                        byte.MaxValue,
                        byte.MaxValue,
                        (byte) 3,
                        (byte) 0
                    };
                    Request[10] = (byte)((uint)Request[0] + (uint)Request[1] + (uint)Request[2] + (uint)Request[3] + (uint)Request[4] + (uint)Request[5] + (uint)Request[6] + (uint)Request[7] + (uint)Request[8] + (uint)Request[9]);
                    byte[] numArray = SendCommand(Request, true);
                    if (numArray == null)
                        flag = false;
                    if (numArray.Length != 11)
                        flag = false;
                    _controllerCellsLights.Add(key, new Dictionary<byte, bool[]>());
                    for (byte firstCellIndex = _controllers[key].FirstCellIndex; (int)firstCellIndex < (int)_controllers[key].CellsCount; ++firstCellIndex)
                        _controllerCellsLights[key].Add(firstCellIndex, new bool[3]);
                }
                return flag;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }

        public bool ResetAllLights()
        {
            try
            {
                bool flag = true;
                _controllerCellsLights = new Dictionary<byte, Dictionary<byte, bool[]>>();
                foreach (byte key in _controllers.Keys)
                {
                    byte[] Request = new byte[11]
                    {
                        (byte) 2,
                        (byte) ((int) key * 16 + 4),
                        (byte) 50,
                        (byte) 0,
                        (byte) 0,
                        (byte) 0,
                        (byte) 0,
                        (byte) 0,
                        (byte) 0,
                        (byte) 3,
                        (byte) 0
                    };
                    Request[10] = (byte)((uint)Request[0] + (uint)Request[1] + (uint)Request[2] + (uint)Request[3] + (uint)Request[4] + (uint)Request[5] + (uint)Request[6] + (uint)Request[7] + (uint)Request[8] + (uint)Request[9]);
                    byte[] numArray = SendCommand(Request, true);
                    if (numArray == null)
                        flag = false;
                    if (numArray.Length != 11)
                        flag = false;
                    _controllerCellsLights.Add(key, new Dictionary<byte, bool[]>());
                    for (byte firstCellIndex = _controllers[key].FirstCellIndex; (int)firstCellIndex < (int)_controllers[key].CellsCount; ++firstCellIndex)
                        _controllerCellsLights[key].Add(firstCellIndex, new bool[3]);
                }
                return flag;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }

        public bool SetLights(int cellNumber, params bool[] lights)
        {
            try
            {
                for (int index1 = 0; index1 <= _cells.GetUpperBound(0); ++index1)
                {
                    for (int index2 = 0; index2 <= _cells.GetUpperBound(1); ++index2)
                    {
                        if (_cells[index1, index2] == cellNumber)
                        {
                            _controllerCellsLights[(byte)index1][(byte)index2] = lights;
                            byte[] lightsSet = CreateLightsSet((byte)index1, _controllerCellsLights[(byte)index1]);
                            byte[] Request = new byte[11]
                            {
                                (byte) 2,
                                (byte) (index1 * 16 + 4),
                                (byte) 50,
                                lightsSet[0],
                                lightsSet[1],
                                lightsSet[2],
                                lightsSet[3],
                                lightsSet[4],
                                lightsSet[5],
                                (byte) 3,
                                (byte) 0
                            };
                            Request[10] = (byte)((uint)Request[0] + (uint)Request[1] + (uint)Request[2] + (uint)Request[3] + (uint)Request[4] + (uint)Request[5] + (uint)Request[6] + (uint)Request[7] + (uint)Request[8] + (uint)Request[9]);
                            byte[] numArray = SendCommand(Request, true);
                            if (numArray == null || numArray.Length != 11)
                                return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }

        private byte[] CreateLightsSet(byte controllerNum, Dictionary<byte, bool[]> cellsLights)
        {
            byte[] numArray = new byte[6];
            for (byte index = 0; index < (byte)3; ++index)
            {
                byte num1 = (byte)((uint)index * 2U);
                for (byte firstCellIndex = _controllers[controllerNum].FirstCellIndex; (int)firstCellIndex < (int)_controllers[controllerNum].CellsCount; ++firstCellIndex)
                {
                    byte num2 = firstCellIndex < (byte)8 ? num1 : (byte)((int)num1 + 1);
                    byte num3 = firstCellIndex < (byte)8 ? firstCellIndex : (byte)((int)firstCellIndex - 8);
                    if (cellsLights[firstCellIndex][(int)index])
                        numArray[(int)num2] = (byte)((uint)numArray[(int)num2] | (uint)_masks[(int)num3]);
                }
            }
            return numArray;
        }

        public byte[] SendCommand(byte[] Request, bool WaitForResponse = true)
        {
            lock (this)
            {
                try
                {
                    for (int index1 = 0; index1 < 3; ++index1)
                    {
                        if ((_client == null || !_client.Connected) && !OpenDevice())
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            _client.GetStream().Write(Request, 0, Request.Length);
                            if (!WaitForResponse)
                                return (byte[])null;
                            for (int index2 = 0; index2 < 100; ++index2)
                            {
                                Thread.Sleep(10);
                                if (_client.Available > 0)
                                {
                                    Thread.Sleep(100);
                                    NetworkStream stream = _client.GetStream();
                                    byte[] buffer = new byte[_client.Available];
                                    stream.Read(buffer, 0, buffer.Length);
                                    //logger.Debug($"Response={BitConverter.ToString(buffer)}");
                                    return buffer;
                                }
                            }
                            if (index1 < 2)
                                Thread.Sleep(200);
                        }
                    }
                    return (byte[])null;
                }
                catch (ThreadAbortException ex)
                {
                    _logger.Error(ex);
                    return (byte[])null;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    return (byte[])null;
                }
            }
        }
    }
}
