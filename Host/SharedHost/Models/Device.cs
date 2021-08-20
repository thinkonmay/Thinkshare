namespace SharedHost.Models
{
    public class Device
    {
        public DeviceState State;
        public SlaveDeviceInformation Information;
        public Device(SlaveDeviceInformation _infor, DeviceState _state)
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

    public class SlaveDeviceInformation
    {
        public string CPU { get; set; }
        public string GPU { get; set; }
        public int RAMcapacity { get; set; }
        public string OS { get; set; }
        public int ID { get; set; }

        public int? SessionClientID { get; set; }
    }

    public class ClientDeviceCapabilities
    {
        public Codec audioCodec { get; set; }

        public Codec videoCodec { get; set; }

        public QoEMode mode { get; set; }

        public int screenWidth { get; set; }

        public int screenHeight { get; set; }

        public int bitrate { get; set; }
    }
}