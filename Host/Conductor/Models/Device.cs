using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace Conductor.Models
{
    public class Device
    {
        public DeviceState State;
        public DeviceInformation Information;
        public Device(DeviceInformation _infor, DeviceState _state)
        {
            State = _state;
            Information = _infor;
        }
    }

    public class DeviceState
    {
        public int CPUusage;
        public int GPUusage;
        public int RAMusage;
    }

    public class DeviceInformation
    {
        public string CPU;
        public string GPU;
        public int RAMcapacity;
        public string OS;
    }
}