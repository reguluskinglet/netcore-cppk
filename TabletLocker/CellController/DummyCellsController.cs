using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;

namespace TabletLocker.CellController
{
    public class DummyCellsController : ICellsController
    {
        private int[,] _cells = (int[,])null;
        private Dictionary<byte, CellsControllerInfo> _controllers = new Dictionary<byte, CellsControllerInfo>();
        private Dictionary<int, bool?> _doorSensorsState = new Dictionary<int, bool?>();
        private Dictionary<int, bool?> _cellSensorsState = new Dictionary<int, bool?>();
        private System.Timers.Timer Timer;

        public Dictionary<byte, CellsControllerInfo> Controllers => _controllers;
        public Dictionary<int, bool?> DoorSensorsState { get; } = new Dictionary<int, bool?>();
        public Dictionary<int, bool?> CellSensorsState { get; } = new Dictionary<int, bool?>();

        Dictionary<byte, CellsControllerInfo> ICellsController.Controllers => _controllers1;

        public string DeviceName => "CellsController";

        private int? _currentcell;
        private Dictionary<byte, CellsControllerInfo> _controllers1 = new Dictionary<byte, CellsControllerInfo>();

        public string DeviceDriverClassName => nameof(DummyCellsController);

        public string DeviceSerialNumber => "";

        public string DeviceDescription => "";

        public int DeviceNumber { get; }

        public bool OpenDevice()
        {
            return true;
        }

        public void CloseDevice()
        {
        }

        public event SensorStateChangedEventHandler SensorStateChangedEvent;

        public string IPAddress { get; set; }

        public int Port { get; set; }

        public bool BeginScanSensors()
        {
            return true;
        }

        public void EndScanSensors()
        {
        }

        public bool InitCellsMap(int[,] _CellsMap)
        {
            try
            {
                _cells = _CellsMap;
                _controllers = new Dictionary<byte, CellsControllerInfo>();
                _doorSensorsState = new Dictionary<int, bool?>();
                _cellSensorsState = new Dictionary<int, bool?>();
                return true;
            }
            catch (Exception )
            {
                return false;
            }
        }

        public void OnTimer(object source, ElapsedEventArgs e)
        {
            if (_currentcell.HasValue)
            {
                SensorStateChangedEvent?.Invoke(1, _currentcell.Value, false);
            }
            Timer.Stop();
        }

        public bool OpenDoor(int CellNumber)
        {
            try
            {
                _currentcell = CellNumber;
                SensorStateChangedEvent?.Invoke(0, CellNumber, true);

                bool flag = false;
                for (int index1 = 0; index1 <= _cells.GetUpperBound(0); ++index1)
                {
                    for (int index2 = 0; index2 <= _cells.GetUpperBound(1); ++index2)
                    {
                        if (_cells[index1, index2] == CellNumber)
                        {
                            flag = true;
                            //start timer for auto close
                            Timer = new System.Timers.Timer { Interval = 10 * 1000 };
                            Timer.Elapsed += OnTimer;
                            Timer.Start();

                            Thread.Sleep(500);
                        }
                    }
                }
                if (!flag)
                    throw new Exception("Ячейка с номером " + CellNumber + " не найдена в конфигурации.");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        CellsControllerInfo ICellsController.GetControllerInfo(byte ControllerNumber)
        {
            throw new NotImplementedException();
        }

        public CellsControllerInfo GetControllerInfo(byte ControllerNumber)
        {
            try
            {
                byte[] Request = new byte[5]
                {
                    (byte) 2,
                    (byte) ((uint) ControllerNumber * 16U),
                    (byte) 48,
                    (byte) 3,
                    (byte) 0
                };
                Request[4] = (byte)((uint)Request[0] + (uint)Request[1] + (uint)Request[2] + (uint)Request[3]);
                byte[] numArray = SendCommand(Request, true);
                if (numArray == null || numArray.Length != 9)
                    return null;
                return new CellsControllerInfo()
                {
                    CellsCount = 16,
                    FirstCellIndex = 0
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool SetAllLights()
        {
            return true;
        }

        public bool ResetAllLights()
        {
            return true;
        }

        public bool SetLights(int CellNumber, params bool[] Lights)
        {
            return true;
        }

        private byte[] CreateLightsSet(byte ControllerNum, Dictionary<byte, bool[]> CellsLights)
        {
            byte[] numArray = new byte[6];
            return numArray;
        }

        public byte[] SendCommand(byte[] Request, bool WaitForResponse = true)
        {
            return null;
        }
    }

}
