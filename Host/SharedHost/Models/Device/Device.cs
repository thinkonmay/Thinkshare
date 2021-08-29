using Conductor.Models;
using SharedHost.Models.Session;

namespace SharedHost.Models.Device
{
    public class SlaveDeviceInformation
    {
        public SlaveDeviceInformation()
        {           
        }
        public SlaveDeviceInformation(Slave slave)
        {
            GPU = slave.GPU;
            CPU = slave.CPU;
            RAMcapacity = slave.RAMcapacity;
            OS = slave.OS;
            ID = slave.ID;
        }

        public string CPU { get; set; }
        public string GPU { get; set; }
        public int RAMcapacity { get; set; }
        public string OS { get; set; }
        public int ID { get; set; }

        public int? SessionClientID { get; set; }
        public string? serviceState { get; set; }
    }

    public class ClientDeviceCapabilities
    {
        public Codec audioCodec { get; set; }

        public Codec videoCodec { get; set; }

        public QoEMode mode { get; set; }

        public int screenWidth { get; set; }

        public int screenHeight { get; set; }
    }
}