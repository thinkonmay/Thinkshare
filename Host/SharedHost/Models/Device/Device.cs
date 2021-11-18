using Conductor.Models;
using SharedHost.Models.Session;

namespace SharedHost.Models.Device
{
    public enum DeviceType
    {
        WEBAPP,
        WINDOW_APP,
        LINUX_APP,
        MAC_OS_APP,
        ANDROID_APP,
        IOS_APP
    }

    public class DeviceCap
    {
        public int? ID {get;set;}
        
        public DeviceType? device {get;set;}

        public Codec? audioCodec { get; set; }

        public Codec? videoCodec { get; set; }

        public QoEMode? mode { get; set; }

        public int? screenWidth { get; set; }

        public int? screenHeight { get; set; }
    }
}