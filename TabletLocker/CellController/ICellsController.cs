using System.Collections.Generic;

namespace TabletLocker.CellController
{
    public interface ICellsController
    {
        Dictionary<byte, CellsControllerInfo> Controllers { get; }

        Dictionary<int, bool?> DoorSensorsState { get; }

        Dictionary<int, bool?> CellSensorsState { get; }

        string DeviceName { get; }

        string DeviceDriverClassName { get; }

        string DeviceSerialNumber { get; }

        string DeviceDescription { get; }

        string IPAddress { get; set; }

        int Port { get; set; }

        int DeviceNumber { get; }

        bool OpenDevice();

        void CloseDevice();

        event SensorStateChangedEventHandler SensorStateChangedEvent;

        bool BeginScanSensors();

        void EndScanSensors();

        bool InitCellsMap(int[,] _CellsMap);

        bool OpenDoor(int CellNumber);

        CellsControllerInfo GetControllerInfo(byte ControllerNumber);

        bool SetAllLights();

        bool ResetAllLights();

        bool SetLights(int CellNumber, params bool[] Lights);

        byte[] SendCommand(byte[] Request, bool WaitForResponse = true);
    }
}