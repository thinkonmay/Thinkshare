using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace Host.Models
{
	public class Slave
    {
        public DeviceInformation deviceInformation { get; set; }

        public DeviceState deviceState { get; set; }

        public int SlaveID;

        public ConnectionState ConnectionState;
    }

    public class DeviceInformation
    {
        public string CPU;
        public string GPU;
        public int RAMcapacity;
        public string OS;
    }

    public class DeviceState
    {
        public int CPUusage;
        public int GPUusage;
        public int RAMusage;
    }

    public enum ConnectionState
    {
        WAITING_RESPOND,
        CONNECTION_ERROR,

        ATTEMP_TO_RECONNECT,

        CONNECTION_OK
    }
}